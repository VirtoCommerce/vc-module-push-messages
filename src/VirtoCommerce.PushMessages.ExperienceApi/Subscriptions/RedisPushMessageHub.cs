using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Subscriptions
{
    public class RedisPushMessageHub : IPushMessageHub, IDisposable
    {
        private const string _channelName = "EventBroker";

        private static string InstanceId { get; } = $"{Environment.MachineName}_{Guid.NewGuid():N}";

        private static readonly RedisChannel _redisChannel = RedisChannel.Literal(_channelName);
        private readonly object _lock = new();
        private bool _isSubscribed;
        private bool _disposed = false;

        private readonly IConnectionMultiplexer _connection;
        private readonly ISubscriber _subscriber;
        private readonly PushMessageHub _eventBroker;
        private readonly ILogger<RedisPushMessageHub> _log;

        public RedisPushMessageHub(IConnectionMultiplexer connection,
            ISubscriber subscriber,
            PushMessageHub eventBroker,
            ILogger<RedisPushMessageHub> log)
        {
            _connection = connection;
            _subscriber = subscriber;
            _eventBroker = eventBroker;
            _log = log;
            EnsureRedisServerConnection();
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
                        _connection.ConnectionFailed += OnConnectionFailed;
                        _connection.ConnectionRestored += OnConnectionRestored;

                        _subscriber.Subscribe(_redisChannel, OnMessage, CommandFlags.FireAndForget);

                        _isSubscribed = true;
                    }
                }
            }
        }

        private void OnConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            _log.LogInformation("Redis backplane connection restored for instance {InstanceId}. Endpoint is {EndPoint}", InstanceId, e.EndPoint);
        }

        private void OnConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            _log.LogError("Redis disconnected from instance {InstanceId}. Endpoint is {EndPoint}, failure type is {FailureType}", InstanceId, e.EndPoint, e.FailureType);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~RedisPushMessageHub()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _subscriber.Unsubscribe(_redisChannel, null, CommandFlags.FireAndForget);

                    _connection.ConnectionFailed -= OnConnectionFailed;
                    _connection.ConnectionRestored -= OnConnectionRestored;
                }
                _disposed = true;
            }
        }
    }
}
