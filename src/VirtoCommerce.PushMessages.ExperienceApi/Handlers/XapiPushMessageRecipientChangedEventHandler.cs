using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Handlers;

public class XapiPushMessageRecipientChangedEventHandler : IEventHandler<PushMessageRecipientChangedEvent>
{
    private readonly IPushMessageService _pushMessageService;
    private readonly IPushMessageHub _eventBroker;

    public XapiPushMessageRecipientChangedEventHandler(
        IPushMessageService pushMessageService,
        IPushMessageHub eventBroker)
    {
        _pushMessageService = pushMessageService;
        _eventBroker = eventBroker;
    }

    public async Task Handle(PushMessageRecipientChangedEvent message)
    {
        foreach (var (messageId, recipients) in message.GetMessageIdsAndRecipients())
        {
            var pushMessage = await _pushMessageService.GetNoCloneAsync(messageId);

            foreach (var recipient in recipients)
            {
                var expPushMessage = ExpPushMessage.Create(pushMessage, recipient);
                await _eventBroker.AddMessageAsync(expPushMessage);
            }
        }
    }
}
