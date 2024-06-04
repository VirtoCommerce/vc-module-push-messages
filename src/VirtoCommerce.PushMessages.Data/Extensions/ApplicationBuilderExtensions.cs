using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.PushMessages.Data.BackgroundJobs;
using JobSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UsePushMessageJobs(this IApplicationBuilder appBuilder)
    {
        var recurringJobService = appBuilder.ApplicationServices.GetService<IRecurringJobService>();

        recurringJobService.WatchJobSetting(
            new SettingCronJobBuilder()
                .SetEnablerSetting(JobSettings.SendScheduledMessagesRecurringJobEnable)
                .SetCronSetting(JobSettings.SendScheduledMessagesRecurringJobCronExpression)
                .ToJob<PushMessageJobService>(x => x.SendScheduledMessagesRecurringJob(JobCancellationToken.Null))
                .Build());

        recurringJobService.WatchJobSetting(
            new SettingCronJobBuilder()
                .SetEnablerSetting(JobSettings.TrackNewRecipientsRecurringJobEnable)
                .SetCronSetting(JobSettings.TrackNewRecipientsRecurringJobCronExpression)
                .ToJob<PushMessageJobService>(x => x.TrackNewRecipientsRecurringJob(JobCancellationToken.Null))
                .Build());

        return appBuilder;
    }
}
