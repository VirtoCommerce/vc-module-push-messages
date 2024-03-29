using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public class PushMessageHub : IPushMessageHub
    {
        private readonly ReplaySubject<ExpPushMessage> _messageStream = new(0);

        public Task<IObservable<ExpPushMessage>> MessagesAsync(string userId)
        {
            var observable = _messageStream.AsObservable();

            if (!string.IsNullOrEmpty(userId))
            {
                observable = observable.Where(x => x.UserId == userId);
            }

            return Task.FromResult(observable);
        }

        public Task<ExpPushMessage> AddMessageAsync(ExpPushMessage message)
        {
            _messageStream.OnNext(message);

            return Task.FromResult(message);
        }
    }
}
