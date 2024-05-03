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
        foreach (var entry in message.ChangedEntries)
        {
            if (entry.EntryState is EntryState.Modified or EntryState.Added)
            {
                var job = RecurringJobs.FirstOrDefault(job => job.EnableSetting.Name == entry.NewEntry.Name || job.CronSetting.Name == entry.NewEntry.Name);
                if (job != null)
                {
                    await StartStopRecurringJob(job);
                }
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
