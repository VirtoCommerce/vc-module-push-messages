using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Extensions;
using VirtoCommerce.SearchModule.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageAudienceService : IPushMessageAudienceService
{
    private readonly ISettingsManager _settingsManager;
    private readonly IMemberService _memberService;
    private readonly IMemberSearchService _memberSearchService;
    private readonly ISearchPhraseParser _searchPhraseParser;

    public PushMessageAudienceService(
        ISettingsManager settingsManager,
        IMemberService memberService,
        IMemberSearchService memberSearchService,
        ISearchPhraseParser searchPhraseParser)
    {
        _settingsManager = settingsManager;
        _memberService = memberService;
        _memberSearchService = memberSearchService;
        _searchPhraseParser = searchPhraseParser;
    }

    public virtual async Task<PushMessageAudienceResult> ResolveAsync(
        PushMessageAudienceCriteria criteria,
        string messageId = null,
        ISet<string> excludedUserIds = null)
    {
        EnsureQueryParses(criteria.MemberQuery);

        var result = AbstractTypeFactory<PushMessageAudienceResult>.TryCreateInstance();
        // Counting needs the whole walk, but building a recipient per login does not: the
        // estimate asks for counters alone on every keystroke.
        var collect = criteria.Take != 0;
        var recipients = new List<PushMessageRecipient>();
        var userIds = new HashSet<string>(excludedUserIds ?? new HashSet<string>());
        var memberIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        // People whose every login already has the message: excluded, not reached twice.
        var alreadySent = 0;

        var searchCriteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
        searchCriteria.ResponseGroup = MemberResponseGroup.WithSecurityAccounts.ToString();
        searchCriteria.Take = await GetBatchSize();
        var queue = new Queue<Member>();

        if (!criteria.MemberIds.IsNullOrEmpty())
        {
            var members = await _memberService.GetByIdsAsync(criteria.MemberIds.ToArray(), searchCriteria.ResponseGroup);

            result.PickedPeople = members.Count(x => x is IHasSecurityAccounts);
            result.PickedCompanies = members.Length - result.PickedPeople;
            members.Apply(x => EnqueueMember(x, fromCompany: false));
        }

        if (!string.IsNullOrEmpty(criteria.MemberQuery))
        {
            await EnqueueMembers(keyword: criteria.MemberQuery);
        }

        while (queue.TryDequeue(out var member))
        {
            if (member is not IHasSecurityAccounts person)
            {
                // A company found inside a company is expanded in its turn; it is not a person.
                await EnqueueMembers(memberId: member.Id);
                continue;
            }

            var counted = CountPerson(result, member, person, userIds, collect ? recipients : null, messageId);

            if (!counted && excludedUserIds != null && person.SecurityAccounts.Any(x => excludedUserIds.Contains(x.Id)))
            {
                alreadySent++;
            }
        }

        // Derived, so the breakdown the UI renders always adds up.
        result.Overlaps = result.MatchedPeople + result.FoundInCompanies - result.PeopleInScope - alreadySent;

        result.Results = collect
            ? recipients.Skip(criteria.Skip).Take(criteria.Take).ToList()
            : [];

        return result;

        async Task EnqueueMembers(string keyword = null, string memberId = null)
        {
            searchCriteria.Keyword = keyword;
            searchCriteria.MemberId = memberId;
            searchCriteria.DeepSearch = !string.IsNullOrEmpty(keyword);

            var fromCompany = memberId != null;

            await foreach (var searchResult in _memberSearchService.SearchBatchesAsync(searchCriteria))
            {
                foreach (var member in searchResult.Results)
                {
                    EnqueueMember(member, fromCompany);
                }
            }
        }

        // People are counted before the de-duplication check: a person reached twice is what
        // Overlaps reports.
        void EnqueueMember(Member member, bool fromCompany)
        {
            if (member is IHasSecurityAccounts && fromCompany)
            {
                result.FoundInCompanies++;
            }
            else if (member is IHasSecurityAccounts)
            {
                result.MatchedPeople++;
            }

            if (memberIds.Add(member.Id))
            {
                queue.Enqueue(member);
            }
        }
    }

    /// <summary>
    /// Takes the person's logins not yet taken and counts them; returns whether there were any.
    /// Recipients are built only when <paramref name="recipients"/> is given: the estimate counts alone.
    /// </summary>
    private static bool CountPerson(
        PushMessageAudienceResult result,
        Member member,
        IHasSecurityAccounts person,
        HashSet<string> userIds,
        List<PushMessageRecipient> recipients,
        string messageId)
    {
        var added = 0;

        // A login another person already brought in is not taken twice.
        foreach (var user in person.SecurityAccounts.Where(x => userIds.Add(x.Id)))
        {
            recipients?.Add(GetRecipient(messageId, member, user));
            added++;
        }

        if (added == 0)
        {
            return false;
        }

        result.TotalCount += added;
        result.PeopleInScope++;
        result.ExtraLogins += added - 1;

        return true;
    }

    /// <summary>
    /// The phrase parser only logs a syntax error: a phrase it cannot read — an unbalanced quote,
    /// a stray bracket — comes back with no keyword and no filters, and the member search then
    /// matches everyone. That must not be reported as an audience, and must never be sent to.
    /// </summary>
    protected virtual void EnsureQueryParses(string memberQuery)
    {
        if (string.IsNullOrWhiteSpace(memberQuery))
        {
            return;
        }

        var parsed = _searchPhraseParser.Parse(memberQuery);

        if (string.IsNullOrWhiteSpace(parsed.Keyword) && parsed.Filters.IsNullOrEmpty())
        {
            throw new ArgumentException($"The member query \"{memberQuery}\" cannot be parsed.", nameof(memberQuery));
        }
    }

    private static PushMessageRecipient GetRecipient(string messageId, Member member, ApplicationUser user)
    {
        var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
        recipient.MessageId = messageId;
        recipient.MemberId = member.Id;
        recipient.MemberName = member.Name;
        recipient.UserId = user.Id;
        recipient.UserName = user.UserName;

        return recipient;
    }

    private Task<int> GetBatchSize()
    {
        return _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);
    }
}
