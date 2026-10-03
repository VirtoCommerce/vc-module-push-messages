using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Data.BackgroundJobs;
using JobSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the push message background jobs and their recurring schedules. This replaces the former
    /// <c>UsePushMessageJobs</c> application-builder call: the schedules are now DI registrations the background-job
    /// engine picks up, and it re-evaluates them whenever an enabler or cron setting changes.
    /// </summary>
    public static IServiceCollection AddPushMessageJobs(this IServiceCollection services)
    {
        // Registered under its own type too, so the job handlers can call the job methods.
        services.AddSingleton<PushMessageJobService>();
        services.AddSingleton<IPushMessageJobService>(provider => provider.GetRequiredService<PushMessageJobService>());

        // The ids are the ones the Hangfire WatchJobSetting registrations generated ({Type}.{Method}), so on the
        // Hangfire engine these replace the old recurring entries instead of leaving them to call a method that no
        // longer runs as a Hangfire job.
        services.AddRecurringJob<SendScheduledMessagesJobHandler, SendScheduledMessagesJobPayload>(schedule => schedule
            .WithId($"{nameof(PushMessageJobService)}.{nameof(PushMessageJobService.SendScheduledMessagesRecurringJob)}")
            .FromSettings(
                JobSettings.SendScheduledMessagesRecurringJobEnable,
                JobSettings.SendScheduledMessagesRecurringJobCronExpression));

        services.AddRecurringJob<TrackNewRecipientsJobHandler, TrackNewRecipientsJobPayload>(schedule => schedule
            .WithId($"{nameof(PushMessageJobService)}.{nameof(PushMessageJobService.TrackNewRecipientsRecurringJob)}")
            .FromSettings(
                JobSettings.TrackNewRecipientsRecurringJobEnable,
                JobSettings.TrackNewRecipientsRecurringJobCronExpression));

        services.AddBackgroundJob<AddRecipientsJobHandler, AddRecipientsJobPayload>(triggerable: false);

        return services;
    }
}
