using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.BackgroundJobs;

public abstract class RecurringJobService<T> : IRecurringJobService
{
    private readonly ISettingsManager _settingsManager;

    protected IList<RecurringJobDescriptor<T>> RecurringJobs { get; } = [];

    protected RecurringJobService(ISettingsManager settingsManager)
    {
        _settingsManager = settingsManager;
    }

    public async Task StartStopRecurringJobs()
    {
        foreach (var job in RecurringJobs)
        {
            await StartStopRecurringJob(job);
        }
    }

    public virtual async Task Handle(ObjectSettingChangedEvent message)
    {
        foreach (var setting in message.ChangedEntries
                     .Where(x => x.EntryState is EntryState.Modified or EntryState.Added)
                     .Select(x => x.NewEntry))
        {
            var job = RecurringJobs.FirstOrDefault(job => job.EnableSetting.Name == setting.Name || job.CronSetting.Name == setting.Name);
            if (job != null)
            {
                await StartStopRecurringJob(job);
            }
        }
    }

    private async Task StartStopRecurringJob(RecurringJobDescriptor<T> job)
    {
        var recurringJobId = $"{job.Method.DeclaringType?.Name}.{job.Method.Name}";

        if (await _settingsManager.GetValueAsync<bool>(job.EnableSetting))
        {
            var cronExpression = await _settingsManager.GetValueAsync<string>(job.CronSetting);
            RecurringJob.AddOrUpdate(recurringJobId, job.MethodCall, cronExpression);
        }
        else
        {
            RecurringJob.RemoveIfExists(recurringJobId);
            CancelProcessingJobs(job.Method);
        }
    }

    private static void CancelProcessingJobs(MethodInfo method)
    {
        var processingJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);

        foreach (var (jobId, _) in processingJobs.Where(x => x.Value?.Job?.Method == method))
        {
            try
            {
                BackgroundJob.Delete(jobId);
            }
            catch
            {
                // Ignore concurrency exceptions, when somebody else cancelled it as well.
            }
        }
    }
}
