using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddRecurringJobService<TService, TImplementation>(this IServiceCollection serviceCollection)
        where TService : class, IRecurringJobService
        where TImplementation : class, TService
    {
        serviceCollection.AddSingleton<TImplementation>();
        serviceCollection.AddSingleton<TService, TImplementation>();
    }
}
