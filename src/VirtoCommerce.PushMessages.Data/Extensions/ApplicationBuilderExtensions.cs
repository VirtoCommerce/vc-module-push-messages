using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Hangfire;
using VirtoCommerce.PushMessages.Core;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Data.BackgroundJobs;
using VirtoCommerce.PushMessages.Data.Handlers;
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

    public static void UseFirebaseCloudMessaging(this IApplicationBuilder appBuilder, string moduleId)
    {
        var options = appBuilder.ApplicationServices.GetService<IOptions<PushMessageOptions>>().Value;

        if (!options.UseFirebaseCloudMessaging)
        {
            return;
        }

        var json = JsonConvert.SerializeObject(options.FcmSenderOptions);
        var appOptions = new AppOptions { Credential = GoogleCredential.FromJson(json) };
        FirebaseApp.Create(appOptions);

        appBuilder.RegisterEventHandler<PushMessageRecipientChangedEvent, FcmPushMessageRecipientChangedEventHandler>();

        var receiverSettings = options.FcmReceiverOptions.ToSettings();
        var settingsRegistrar = appBuilder.ApplicationServices.GetRequiredService<ISettingsRegistrar>();
        settingsRegistrar.RegisterSettings(receiverSettings, moduleId);
        settingsRegistrar.RegisterSettingsForType(receiverSettings, "Store");
    }

    private static IList<SettingDescriptor> ToSettings(this FcmReceiverOptions options)
    {
        var settings = options
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(x => CreateSetting(x.Name, x.GetValue(options)))
            .ToList();

        settings.Insert(0, ModuleConstants.Settings.General.FirebaseCloudMessagingEnable);

        return settings;
    }

    private static SettingDescriptor CreateSetting(string name, object value)
    {
        return new SettingDescriptor
        {
            Name = $"PushMessages.{nameof(FcmReceiverOptions)}.{name}",
            GroupName = "Push Messages|FCM Receiver Options",
            ValueType = SettingValueType.ShortText,
            DefaultValue = value,
            IsPublic = true,
        };
    }
}
