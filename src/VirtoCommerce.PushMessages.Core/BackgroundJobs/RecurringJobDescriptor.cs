using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.PushMessages.Core.BackgroundJobs;

public class RecurringJobDescriptor<T>
{
    public SettingDescriptor EnableSetting { get; set; }
    public SettingDescriptor CronSetting { get; set; }
    public MethodInfo Method { get; set; }
    public Expression<Func<T, Task>> MethodCall { get; set; }
}
