using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDistributedMessageService(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnectionString = configuration.GetConnectionString("RedisConnectionString");
            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddSingleton<PushMessageHub>();
                services.AddSingleton<IPushMessageHub, RedisPushMessageHub>();
            }
            else
            {
                services.AddSingleton<IPushMessageHub, PushMessageHub>();
            }

            return services;
        }
    }
}
