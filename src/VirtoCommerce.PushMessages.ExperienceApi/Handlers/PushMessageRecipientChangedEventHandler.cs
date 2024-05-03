using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Handlers;

public class PushMessageRecipientChangedEventHandler : IEventHandler<PushMessageRecipientChangedEvent>
{
    private readonly IPushMessageService _pushMessageService;
    private readonly IPushMessageHub _eventBroker;

    public PushMessageRecipientChangedEventHandler(
        IPushMessageService pushMessageService,
        IPushMessageHub eventBroker)
    {
        _pushMessageService = pushMessageService;
        _eventBroker = eventBroker;
    }

    public async Task Handle(PushMessageRecipientChangedEvent message)
    {
        foreach (var (messageId, userIds) in message.ChangedEntries
                     .Where(x => x.EntryState == EntryState.Added)
                     .GroupBy(x => x.NewEntry.MessageId)
                     .ToDictionary(g => g.Key, g => g.Select(x => x.NewEntry.UserId)))
        {
            var pushMessage = await _pushMessageService.GetNoCloneAsync(messageId);

            foreach (var userId in userIds)
            {
                var expPushMessage = AbstractTypeFactory<ExpPushMessage>.TryCreateInstance();
                expPushMessage.Id = pushMessage.Id;
                expPushMessage.ShortMessage = pushMessage.ShortMessage;
                expPushMessage.CreatedDate = pushMessage.CreatedDate;
                expPushMessage.UserId = userId;

                await _eventBroker.AddMessageAsync(expPushMessage);
            }
        }
    }
}
