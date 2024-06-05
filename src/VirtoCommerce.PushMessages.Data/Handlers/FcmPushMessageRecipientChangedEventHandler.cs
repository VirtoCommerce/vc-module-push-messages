using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.Data.Handlers;

public class FcmPushMessageRecipientChangedEventHandler : IEventHandler<PushMessageRecipientChangedEvent>
{
    private readonly IPushMessageService _pushMessageService;
    private readonly IFcmTokenSearchService _fcmTokenSearchService;
    private readonly ISettingsManager _settingsManager;
    private readonly ILogger<FcmPushMessageRecipientChangedEventHandler> _logger;

    public FcmPushMessageRecipientChangedEventHandler(
        IPushMessageService pushMessageService,
        IFcmTokenSearchService fcmTokenSearchService,
        ISettingsManager settingsManager,
        ILogger<FcmPushMessageRecipientChangedEventHandler> logger)
    {
        _pushMessageService = pushMessageService;
        _fcmTokenSearchService = fcmTokenSearchService;
        _settingsManager = settingsManager;
        _logger = logger;
    }

    public async Task Handle(PushMessageRecipientChangedEvent message)
    {
        foreach (var (messageId, recipients) in message.GetMessageIdsAndRecipients())
        {
            var pushMessage = await _pushMessageService.GetNoCloneAsync(messageId);
            await SendMessageAsync(pushMessage, recipients);
        }
    }

    public async Task SendMessageAsync(PushMessage message, IList<PushMessageRecipient> recipients)
    {
        var firebaseMessage = new MulticastMessage
        {
            Data = new Dictionary<string, string>
            {
                { "messageId", message.Id },
            },
            Notification = new Notification
            {
                Body = message.ShortMessage,
            },
        };

        var searchCriteria = AbstractTypeFactory<FcmTokenSearchCriteria>.TryCreateInstance();
        searchCriteria.UserIds = recipients.Select(x => x.UserId).ToList();
        searchCriteria.Take = await GetBatchSize();

        await foreach (var searchResult in _fcmTokenSearchService.SearchBatchesNoCloneAsync(searchCriteria))
        {
            firebaseMessage.Tokens = searchResult.Results.Select(x => x.Token).ToArray();
            await SendFirebaseMessage(firebaseMessage);
        }
    }

    private async Task SendFirebaseMessage(MulticastMessage firebaseMessage)
    {
        try
        {
            var batchResponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(firebaseMessage);

            if (batchResponse.FailureCount == 0)
            {
                return;
            }

            foreach (var response in batchResponse.Responses.Where(x => !x.IsSuccess))
            {
                _logger.LogError("FCM Send failed: {Exception}", response.Exception);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("FCM Send failed. {Exception}", ex);
        }
    }

    private Task<int> GetBatchSize()
    {
        return _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);
    }
}
