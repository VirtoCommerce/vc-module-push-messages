using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public class PushMessageHub : IPushMessageHub
    {
        private readonly ISubject<ExpPushMessage> _messageStream = new ReplaySubject<ExpPushMessage>(0);

        public Task<IObservable<ExpPushMessage>> MessagesAsync()
        {
            var observable = _messageStream.AsObservable();
            return Task.FromResult(observable);
        }

        public Task<IObservable<ExpPushMessage>> MessagesByUserIdAsync(string userId)
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
