using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Extensions;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Data.BackgroundJobs;

public class PushMessageJobService : IPushMessageJobService
{
    private readonly ISettingsManager _settingsManager;
    private readonly IPushMessageService _messageService;
    private readonly IPushMessageSearchService _messageSearchService;
    private readonly IPushMessageRecipientService _recipientService;
    private readonly IPushMessageRecipientSearchService _recipientSearchService;
    private readonly IMemberService _memberService;
    private readonly IMemberSearchService _memberSearchService;

    public PushMessageJobService(
        ISettingsManager settingsManager,
        IPushMessageService messageService,
        IPushMessageSearchService messageSearchService,
        IPushMessageRecipientService recipientService,
        IPushMessageRecipientSearchService recipientSearchService,
        IMemberService memberService,
        IMemberSearchService memberSearchService)
    {
        _settingsManager = settingsManager;
        _messageService = messageService;
        _messageSearchService = messageSearchService;
        _recipientService = recipientService;
        _recipientSearchService = recipientSearchService;
        _memberService = memberService;
        _memberSearchService = memberSearchService;
    }

    public void EnqueueAddRecipients(IList<string> messageIds = null)
    {
        if (messageIds?.Count > 0)
        {
            BackgroundJob.Enqueue<PushMessageJobService>(x => x.AddRecipientsJob(messageIds, JobCancellationToken.Null));
        }
        else
        {
            BackgroundJob.Enqueue<PushMessageJobService>(x => x.TrackNewRecipientsRecurringJob(JobCancellationToken.Null));
        }
    }

    [DisableConcurrentExecution(10)]
    public async Task SendScheduledMessagesRecurringJob(IJobCancellationToken cancellationToken)
    {
        var searchCriteria = AbstractTypeFactory<PushMessageSearchCriteria>.TryCreateInstance();
        searchCriteria.Statuses = [PushMessageStatus.Scheduled];
        searchCriteria.StartDateBefore = DateTime.UtcNow;
        searchCriteria.Sort = $"{nameof(PushMessage.StartDate)};{nameof(PushMessage.CreatedDate)}";
        searchCriteria.Take = await GetBatchSize();

        await _messageSearchService.SearchWhileResultIsNotEmpty(searchCriteria, async searchResult =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            searchResult.Results.Apply(x => x.Status = PushMessageStatus.Sent);
            await _messageService.SaveChangesAsync(searchResult.Results);
        });
    }

    [DisableConcurrentExecution(10)]
    public async Task TrackNewRecipientsRecurringJob(IJobCancellationToken cancellationToken)
    {
        var searchCriteria = AbstractTypeFactory<PushMessageSearchCriteria>.TryCreateInstance();
        searchCriteria.Statuses = [PushMessageStatus.Sent];
        searchCriteria.TrackNewRecipients = true;
        searchCriteria.CreatedDateBefore = DateTime.UtcNow;
        searchCriteria.ResponseGroup = PushMessageResponseGroup.WithMembers.ToString();
        searchCriteria.Take = await GetBatchSize();

        await foreach (var searchResult in _messageSearchService.SearchBatchesNoCloneAsync(searchCriteria))
        {
            foreach (var message in searchResult.Results)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await AddRecipients(message);
            }
        }
    }

    public async Task AddRecipientsJob(IList<string> messageIds, IJobCancellationToken cancellationToken)
    {
        foreach (var messageId in messageIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var message = await _messageService.GetNoCloneAsync(messageId, PushMessageResponseGroup.WithMembers.ToString());
            if (message != null)
            {
                await AddRecipients(message);
            }
        }
    }

    private async Task AddRecipients(PushMessage message)
    {
        var oldUserIds = await GetExistingRecipientUserIds(message.Id);
        var recipients = await GetNewRecipients(message, oldUserIds);

        if (recipients.Count > 0)
        {
            await _recipientService.SaveChangesAsync(recipients);
        }
    }

    private async Task<HashSet<string>> GetExistingRecipientUserIds(string messageId)
    {
        var searchCriteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
        searchCriteria.MessageId = messageId;
        searchCriteria.WithHidden = true;
        searchCriteria.Take = await GetBatchSize();

        var userIds = new HashSet<string>();

        await foreach (var searchResult in _recipientSearchService.SearchBatchesNoCloneAsync(searchCriteria))
        {
            searchResult.Results.Apply(x => userIds.Add(x.UserId));
        }

        return userIds;
    }

    private async Task<IList<PushMessageRecipient>> GetNewRecipients(PushMessage message, HashSet<string> userIds)
    {
        var recipients = new List<PushMessageRecipient>();
        var memberIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var searchCriteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
        searchCriteria.ResponseGroup = MemberResponseGroup.WithSecurityAccounts.ToString();
        searchCriteria.Take = await GetBatchSize();
        var queue = new Queue<Member>();

        if (!message.MemberIds.IsNullOrEmpty())
        {
            var members = await _memberService.GetByIdsAsync(message.MemberIds.ToArray(), searchCriteria.ResponseGroup);
            members.Apply(EnqueueMember);
        }

        if (!string.IsNullOrEmpty(message.MemberQuery))
        {
            await EnqueueMembers(keyword: message.MemberQuery);
        }

        while (queue.TryDequeue(out var member))
        {
            if (member is IHasSecurityAccounts hasSecurityAccounts)
            {
                foreach (var user in hasSecurityAccounts.SecurityAccounts)
                {
                    AddRecipient(member, user);
                }
            }
            else
            {
                await EnqueueMembers(memberId: member.Id);
            }
        }

        return recipients;

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
            }
        }

        void AddRecipient(Member member, ApplicationUser user)
        {
            if (userIds.Add(user.Id))
            {
                recipients.Add(GetRecipient(message, member, user));
            }
        }
    }

    private static PushMessageRecipient GetRecipient(PushMessage message, Member member, ApplicationUser user)
    {
        var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
        recipient.MessageId = message.Id;
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
