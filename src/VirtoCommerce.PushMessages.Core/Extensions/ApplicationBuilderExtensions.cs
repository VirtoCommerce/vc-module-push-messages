using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Core.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void UseRecurringJobService<T>(this IApplicationBuilder appBuilder)
        where T : class, IRecurringJobService
    {
        appBuilder.RegisterEventHandler<ObjectSettingChangedEvent, T>();

        var pushMessageJobService = appBuilder.ApplicationServices.GetService<T>();
        pushMessageJobService.StartStopRecurringJobs().GetAwaiter().GetResult();
    }
}
