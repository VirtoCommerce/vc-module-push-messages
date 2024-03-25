using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Handlers
{
    public class PushMessageChangedEventHandler : IEventHandler<PushMessageChangedEvent>
    {
        private readonly IPushMessageHub _eventBroker;

        public PushMessageChangedEventHandler(IPushMessageHub eventBroker)
        {
            _eventBroker = eventBroker;
        }

        public async Task Handle(PushMessageChangedEvent message)
        {
            foreach (var entry in message.ChangedEntries)
            {
                var pushMessage = entry.NewEntry;

                foreach (var userId in pushMessage.UserIds)
                {
                    var expPushMessage = new ExpPushMessage
                    {
                        Id = pushMessage.Id,
                        ShortMessage = pushMessage.ShortMessage,
                        CreatedDate = pushMessage.CreatedDate,
                        UserId = userId,
                    };

                    await _eventBroker.AddMessageAsync(expPushMessage);
                }
            }
        }
    }
}
