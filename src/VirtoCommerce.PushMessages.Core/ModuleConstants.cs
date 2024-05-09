using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PushMessages.Core;

public static class ModuleConstants
{
    public static class Security
    {
        public static class Permissions
        {
            public const string Access = "PushMessages:access";
            public const string Create = "PushMessages:create";
            public const string Read = "PushMessages:read";
            public const string Update = "PushMessages:update";
            public const string Delete = "PushMessages:delete";

            public static string[] AllPermissions { get; } =
            [
                Access,
                Create,
                Read,
                Update,
                Delete,
            ];
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor BatchSize { get; } = new()
            {
                Name = "PushMessages.BatchSize",
                GroupName = "Push Messages|General",
                ValueType = SettingValueType.Integer,
                DefaultValue = 50,
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return BatchSize;
                }
            }
        }

        public static class BackgroundJobs
        {
            public static SettingDescriptor SendScheduledMessagesRecurringJobEnable { get; } = new()
            {
                Name = "PushMessages.SendScheduledMessagesRecurringJob.Enable",
                GroupName = "Push Messages|Background Jobs",
                ValueType = SettingValueType.Boolean,
                DefaultValue = true,
            };

            public static SettingDescriptor SendScheduledMessagesRecurringJobCronExpression { get; } = new()
            {
                Name = "PushMessages.SendScheduledMessagesRecurringJob.CronExpression",
                GroupName = "Push Messages|Background Jobs",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "0/5 * * * *",
            };

            public static SettingDescriptor TrackNewRecipientsRecurringJobEnable { get; } = new()
            {
                Name = "PushMessages.TrackNewRecipientsRecurringJob.Enable",
                GroupName = "Push Messages|Background Jobs",
                ValueType = SettingValueType.Boolean,
                DefaultValue = true,
            };

            public static SettingDescriptor TrackNewRecipientsRecurringJobCronExpression { get; } = new()
            {
                Name = "PushMessages.TrackNewRecipientsRecurringJob.CronExpression",
                GroupName = "Push Messages|Background Jobs",
                ValueType = SettingValueType.ShortText,
                DefaultValue = "0 0/1 * * *",
            };

            public static IEnumerable<SettingDescriptor> AllBackgroundJobsSettings
            {
                get
                {
                    yield return SendScheduledMessagesRecurringJobEnable;
                    yield return SendScheduledMessagesRecurringJobCronExpression;
                    yield return TrackNewRecipientsRecurringJobEnable;
                    yield return TrackNewRecipientsRecurringJobCronExpression;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings.Concat(BackgroundJobs.AllBackgroundJobsSettings);
            }
        }
    }
}
