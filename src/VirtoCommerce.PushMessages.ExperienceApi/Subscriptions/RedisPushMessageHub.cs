using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public class RedisPushMessageHub : IPushMessageHub
    {
        private readonly PushMessageHub _eventBroker;

        private const string ChannelName = "EventBroker";

        private static readonly RedisChannel _reddisChannel = RedisChannel.Literal(ChannelName);

        private readonly object _lock = new object();

        private bool _isSubscribed;

        private readonly IConnectionMultiplexer _connection;
        private readonly ISubscriber _subscriber;

        public RedisPushMessageHub(IConnectionMultiplexer connection, ISubscriber subscriber, PushMessageHub eventBroker)
        {
            _connection = connection;
            _subscriber = subscriber;
            _eventBroker = eventBroker;
        }

        public async Task<ExpPushMessage> AddMessageAsync(ExpPushMessage message)
        {
            EnsureRedisServerConnection();

            await _subscriber.PublishAsync(_reddisChannel, JsonConvert.SerializeObject(message), CommandFlags.FireAndForget);

            return message;
        }

        public Task<IObservable<ExpPushMessage>> MessagesAsync()
        {
            return _eventBroker.MessagesAsync();
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
                        _subscriber.Subscribe(_reddisChannel, OnMessage, CommandFlags.FireAndForget);

                        _isSubscribed = true;
                    }
                }
            }

        }

    }
}
