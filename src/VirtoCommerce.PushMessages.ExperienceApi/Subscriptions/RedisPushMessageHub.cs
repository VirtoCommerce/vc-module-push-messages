using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public class RedisPushMessageHub : IPushMessageHub
    {
        private const string _channelName = "EventBroker";
        private static readonly RedisChannel _redisChannel = RedisChannel.Literal(_channelName);
        private readonly object _lock = new();
        private bool _isSubscribed;

        private readonly ISubscriber _subscriber;
        private readonly PushMessageHub _eventBroker;

        public RedisPushMessageHub(ISubscriber subscriber, PushMessageHub eventBroker)
        {
            _subscriber = subscriber;
            _eventBroker = eventBroker;
        }

        public async Task<ExpPushMessage> AddMessageAsync(ExpPushMessage message)
        {
            EnsureRedisServerConnection();

            await _subscriber.PublishAsync(_redisChannel, JsonConvert.SerializeObject(message), CommandFlags.FireAndForget);

            return message;
        }

        public Task<IObservable<ExpPushMessage>> MessagesAsync(string userId)
        {
            return _eventBroker.MessagesAsync(userId);
        }

        protected virtual void OnMessage(RedisChannel channel, RedisValue redisValue)
        {
            var message = JsonConvert.DeserializeObject<ExpPushMessage>(redisValue);

            if (message?.UserId != null)
            {
                _eventBroker.AddMessageAsync(message);
            }
        }

        private void EnsureRedisServerConnection()
        {
            if (!_isSubscribed)
            {
                lock (_lock)
                {
                    if (!_isSubscribed)
                    {
                        _subscriber.Subscribe(_redisChannel, OnMessage, CommandFlags.FireAndForget);

                        _isSubscribed = true;
                    }
                }
            }
        }
    }
}
