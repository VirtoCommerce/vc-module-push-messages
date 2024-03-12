using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Handlers
{
    public class PushMessageSendingEventHandler : IEventHandler<PushMessageSendingEvent>
    {
        private readonly IPushMessageHub _eventBroker;

        public PushMessageSendingEventHandler(IPushMessageHub eventBroker)
        {
            _eventBroker = eventBroker;
        }

        public async Task Handle(PushMessageSendingEvent message)
        {
            foreach (var entry in message.ChangedEntries)
            {
                var pushMessage = entry.NewEntry;

                foreach (var recipient in pushMessage.Recipients)
                {
                    var expPushMessage = new ExpPushMessage
                    {
                        Id = pushMessage.Message.Id,
                        ShortMessage = pushMessage.Message.ShortMessage,
                        CreatedDate = pushMessage.Message.CreatedDate,
                        UserId = recipient.UserId,
                    };

                    await _eventBroker.AddMessageAsync(expPushMessage);
                }
            }
        }
    }
}
