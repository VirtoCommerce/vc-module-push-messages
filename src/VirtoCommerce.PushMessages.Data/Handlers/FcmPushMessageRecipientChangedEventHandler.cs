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
    private readonly IFcmTokenService _fcmTokenService;
    private readonly ISettingsManager _settingsManager;
    private readonly ILogger<FcmPushMessageRecipientChangedEventHandler> _logger;

    public FcmPushMessageRecipientChangedEventHandler(
        IPushMessageService pushMessageService,
        IFcmTokenSearchService fcmTokenSearchService,
        IFcmTokenService fcmTokenService,
        ISettingsManager settingsManager,
        ILogger<FcmPushMessageRecipientChangedEventHandler> logger)
    {
        _pushMessageService = pushMessageService;
        _fcmTokenSearchService = fcmTokenSearchService;
        _fcmTokenService = fcmTokenService;
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
                { "body", message.ShortMessage },
            },
        };

        var searchCriteria = AbstractTypeFactory<FcmTokenSearchCriteria>.TryCreateInstance();
        searchCriteria.UserIds = recipients.Select(x => x.UserId).ToList();
        searchCriteria.Take = await GetBatchSize();

        await foreach (var searchResult in _fcmTokenSearchService.SearchBatchesNoCloneAsync(searchCriteria))
        {
            var tokens = searchResult.Results;
            firebaseMessage.Tokens = tokens.Select(x => x.Token).ToArray();
            await SendFirebaseMessage(firebaseMessage, tokens);
        }
    }

    private async Task SendFirebaseMessage(MulticastMessage firebaseMessage, IList<FcmToken> tokens)
    {
        try
        {
            var responses = await SendEachForMulticastAsync(firebaseMessage);

            var staleTokenIds = new List<string>();

            // Responses are returned in the same order as the tokens that were sent,
            // so response[i] corresponds to tokens[i].
            for (var i = 0; i < responses.Count && i < tokens.Count; i++)
            {
                var response = responses[i];
                if (response.IsSuccess)
                {
                    continue;
                }

                _logger.LogError("FCM Send failed: {ErrorCode}", response.ErrorCode);

                if (IsStaleTokenError(response.ErrorCode))
                {
                    staleTokenIds.Add(tokens[i].Id);
                }
            }

            if (staleTokenIds.Count > 0)
            {
                await _fcmTokenService.DeleteAsync(staleTokenIds);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("FCM Send failed. {Exception}", ex);
        }
    }

    /// <summary>
    /// Per-token terminal errors that mean the registration token is permanently
    /// invalid and must be pruned so it is not retried on every subsequent push.
    /// Transient errors (Unavailable, Internal, QuotaExceeded, etc.) are NOT pruned.
    /// </summary>
    private static bool IsStaleTokenError(MessagingErrorCode? errorCode)
    {
        return errorCode is MessagingErrorCode.Unregistered or MessagingErrorCode.InvalidArgument;
    }

    /// <summary>
    /// Seam over the FirebaseAdmin static singleton so the dead-token pruning
    /// behavior can be unit-tested without constructing the non-public
    /// <c>BatchResponse</c>/<c>SendResponse</c> types.
    /// </summary>
    protected virtual async Task<IList<FcmSendResult>> SendEachForMulticastAsync(MulticastMessage firebaseMessage)
    {
        var batchResponse = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(firebaseMessage);

        return batchResponse.Responses
            .Select(x => new FcmSendResult
            {
                IsSuccess = x.IsSuccess,
                ErrorCode = (x.Exception as FirebaseMessagingException)?.MessagingErrorCode,
            })
            .ToArray();
    }

    private Task<int> GetBatchSize()
    {
        return _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);
    }
}
