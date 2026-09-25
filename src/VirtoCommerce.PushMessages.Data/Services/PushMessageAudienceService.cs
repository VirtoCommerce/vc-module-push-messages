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
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageAudienceService : IPushMessageAudienceService
{
    private readonly ISettingsManager _settingsManager;
    private readonly IMemberService _memberService;
    private readonly IMemberSearchService _memberSearchService;

    public PushMessageAudienceService(
        ISettingsManager settingsManager,
        IMemberService memberService,
        IMemberSearchService memberSearchService)
    {
        _settingsManager = settingsManager;
        _memberService = memberService;
        _memberSearchService = memberSearchService;
    }

    public virtual async Task<PushMessageAudienceResult> ResolveAsync(
        PushMessageAudienceCriteria criteria,
        string messageId = null,
        ISet<string> excludedUserIds = null)
    {
        var result = AbstractTypeFactory<PushMessageAudienceResult>.TryCreateInstance();
        // Counting needs the whole walk, but building a recipient per login does not: the
        // estimate asks for counters alone on every keystroke.
        var collect = criteria.Take != 0;
        var recipients = new List<PushMessageRecipient>();
        var userIds = new HashSet<string>(excludedUserIds ?? new HashSet<string>());
        var memberIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var people = 0;

        var searchCriteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
        searchCriteria.ResponseGroup = MemberResponseGroup.WithSecurityAccounts.ToString();
        searchCriteria.Take = await GetBatchSize();
        var queue = new Queue<Member>();

        if (!criteria.MemberIds.IsNullOrEmpty())
        {
            var members = await _memberService.GetByIdsAsync(criteria.MemberIds.ToArray(), searchCriteria.ResponseGroup);
            members.Apply(EnqueueMember);
        }

        if (!string.IsNullOrEmpty(criteria.MemberQuery))
        {
            await EnqueueMembers(keyword: criteria.MemberQuery);
        }

        result.MembersMatched = memberIds.Count;

        while (queue.TryDequeue(out var member))
        {
            if (member is IHasSecurityAccounts hasSecurityAccounts)
            {
                var added = 0;

                foreach (var user in hasSecurityAccounts.SecurityAccounts)
                {
                    if (userIds.Add(user.Id))
                    {
                        if (collect)
                        {
                            recipients.Add(GetRecipient(messageId, member, user));
                        }

                        added++;
                    }
                }

                if (added > 0)
                {
                    result.TotalCount += added;
                    result.PeopleInScope++;
                    result.ExtraLogins += added - 1;
                }
            }
            else
            {
                // A company found inside a company is expanded in its turn; it is not a person.
                var before = people;
                await EnqueueMembers(memberId: member.Id);

                result.CompaniesExpanded++;
                result.PeopleFromCompanies += people - before;
            }
        }

        result.Results = collect
            ? recipients.Skip(criteria.Skip).Take(criteria.Take).ToList()
            : [];

        return result;

        async Task EnqueueMembers(string keyword = null, string memberId = null)
        {
            searchCriteria.Keyword = keyword;
            searchCriteria.MemberId = memberId;
            searchCriteria.DeepSearch = !string.IsNullOrEmpty(keyword);

            await foreach (var searchResult in _memberSearchService.SearchBatchesAsync(searchCriteria))
            {
                searchResult.Results.Apply(EnqueueMember);
            }
        }

        void EnqueueMember(Member member)
        {
            if (memberIds.Add(member.Id))
            {
                queue.Enqueue(member);

                if (member is IHasSecurityAccounts)
                {
                    people++;
                }
            }
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
