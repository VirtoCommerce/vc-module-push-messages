using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Data.BackgroundJobs;

public class PushMessageJobService : IPushMessageJobService
{
    private readonly ISettingsManager _settingsManager;
    private readonly IPushMessageService _messageService;
    private readonly IPushMessageSearchService _messageSearchService;
    private readonly IPushMessageRecipientService _recipientService;
    private readonly IPushMessageRecipientSearchService _recipientSearchService;
    private readonly IPushMessageAudienceService _audienceService;

    public PushMessageJobService(
        ISettingsManager settingsManager,
        IPushMessageService messageService,
        IPushMessageSearchService messageSearchService,
        IPushMessageRecipientService recipientService,
        IPushMessageRecipientSearchService recipientSearchService,
        IPushMessageAudienceService audienceService)
    {
        _settingsManager = settingsManager;
        _messageService = messageService;
        _messageSearchService = messageSearchService;
        _recipientService = recipientService;
        _recipientSearchService = recipientSearchService;
        _audienceService = audienceService;
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

        var criteria = AbstractTypeFactory<PushMessageAudienceCriteria>.TryCreateInstance();
        criteria.MemberQuery = message.MemberQuery;
        criteria.MemberIds = message.MemberIds;
        // The job saves every recipient, so it never pages.
        criteria.Take = int.MaxValue;

        var audience = await _audienceService.ResolveAsync(criteria, message.Id, oldUserIds);

        if (audience.Results.Count > 0)
        {
            await _recipientService.SaveChangesAsync(audience.Results);
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

    private Task<int> GetBatchSize()
    {
        return _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);
    }
}
