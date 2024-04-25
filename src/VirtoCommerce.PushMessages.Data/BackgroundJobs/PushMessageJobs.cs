using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Hangfire;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;
using JobSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.BackgroundJobs;

public class PushMessageJobs : IPushMessageJobService
{
    private static readonly MethodInfo _recurringJobMethod = typeof(PushMessageJobs).GetMethod(nameof(SendScheduledMessages));

    private readonly IPushMessageService _crudService;
    private readonly IPushMessageSearchService _searchService;
    private readonly ISettingsManager _settingsManager;

    public PushMessageJobs(
        IPushMessageService crudService,
        IPushMessageSearchService searchService,
        ISettingsManager settingsManager)
    {
        _crudService = crudService;
        _searchService = searchService;
        _settingsManager = settingsManager;
    }

    [DisableConcurrentExecution(10)]
    public async Task SendScheduledMessages(IJobCancellationToken cancellationToken)
    {
        var searchCriteria = AbstractTypeFactory<PushMessageSearchCriteria>.TryCreateInstance();
        searchCriteria.StartDateBefore = DateTime.UtcNow;
        searchCriteria.Statuses = [PushMessageStatus.Scheduled];
        searchCriteria.Sort = $"{nameof(PushMessage.StartDate)};{nameof(PushMessage.CreatedDate)}";
        searchCriteria.Take = await _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);

        await _searchService.SearchWhileResultIsNotEmpty(searchCriteria, async searchResult =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            searchResult.Results.Apply(x => x.Status = PushMessageStatus.Sent);
            await _crudService.SaveChangesAsync(searchResult.Results);
        });
    }

    public async Task StartStopRecurringJobs()
    {
        const string recurringJobId = $"{nameof(PushMessageJobs)}.{nameof(SendScheduledMessages)}";
        var enableJobs = await _settingsManager.GetValueAsync<bool>(JobSettings.Enable);

        if (enableJobs)
        {
            var cronExpression = await _settingsManager.GetValueAsync<string>(JobSettings.CronExpression);
            RecurringJob.AddOrUpdate<PushMessageJobs>(recurringJobId, x => x.SendScheduledMessages(JobCancellationToken.Null), cronExpression);
        }
        else
        {
            CancelProcessingJobs(_recurringJobMethod);
            RecurringJob.RemoveIfExists(recurringJobId);
        }
    }


    private static void CancelProcessingJobs(MethodInfo method)
    {
        var processingJobs = JobStorage.Current.GetMonitoringApi().ProcessingJobs(0, int.MaxValue);
        var (jobId, _) = processingJobs.FirstOrDefault(x => x.Value?.Job?.Method == method);

        if (string.IsNullOrEmpty(jobId))
        {
            return;
        }

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
