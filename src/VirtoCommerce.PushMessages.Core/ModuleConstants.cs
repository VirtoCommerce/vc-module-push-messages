using System.Collections.Generic;
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
            {
                Access,
                Create,
                Read,
                Update,
                Delete,
            };
        }
    }

    public static class Settings
    {
        public static class General
        {
            public static SettingDescriptor PushMessagesEnabled { get; } = new()
            {
                Name = "PushMessages.PushMessagesEnabled",
                GroupName = "PushMessages|General",
                ValueType = SettingValueType.Boolean,
                DefaultValue = false,
            };

            public static SettingDescriptor PushMessagesPassword { get; } = new()
            {
                Name = "PushMessages.PushMessagesPassword",
                GroupName = "PushMessages|Advanced",
                ValueType = SettingValueType.SecureString,
                DefaultValue = "qwerty",
            };

            public static IEnumerable<SettingDescriptor> AllGeneralSettings
            {
                get
                {
                    yield return PushMessagesEnabled;
                    yield return PushMessagesPassword;
                }
            }
        }

        public static IEnumerable<SettingDescriptor> AllSettings
        {
            get
            {
                return General.AllGeneralSettings;
            }
        }
    }
}
