using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.DistributedLock;
using VirtoCommerce.Platform.Core.Jobs;

namespace VirtoCommerce.PushMessages.Data.BackgroundJobs;

// The handlers below run the PushMessageJobService methods that used to be Hangfire jobs. Where the Hangfire method
// carried [DisableConcurrentExecution(10)], the handler takes a distributed lock with the same 10-second wait and, like
// Hangfire, fails when it times out so the engine retries the job. Skipping instead could lose a run: the track-new-
// recipients job is also started by member changes, not only by its schedule.

public class SendScheduledMessagesJobPayload
{
}

/// <summary>
/// Marks scheduled push messages whose start date has passed as sent. Runs on the recurring schedule.
/// </summary>
public class SendScheduledMessagesJobHandler(PushMessageJobService jobService, IDistributedLock distributedLock)
    : IBackgroundJobHandler<SendScheduledMessagesJobPayload>
{
    private const string LockResource = "push-messages:job:send-scheduled-messages";
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public virtual Task Execute(SendScheduledMessagesJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, jobService.SendScheduledMessagesRecurringJob, _lockTimeout, cancellationToken);
    }
}

public class TrackNewRecipientsJobPayload
{
}

/// <summary>
/// Adds recipients that joined after a message was sent, for messages that track new recipients. Runs on the
/// recurring schedule and whenever a member is added or updated.
/// </summary>
public class TrackNewRecipientsJobHandler(PushMessageJobService jobService, IDistributedLock distributedLock)
    : IBackgroundJobHandler<TrackNewRecipientsJobPayload>
{
    private const string LockResource = "push-messages:job:track-new-recipients";
    private static readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public virtual Task Execute(TrackNewRecipientsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return distributedLock.ExecuteAsync(LockResource, jobService.TrackNewRecipientsRecurringJob, _lockTimeout, cancellationToken);
    }
}

public class AddRecipientsJobPayload
{
    public IList<string> MessageIds { get; set; } = [];
}

/// <summary>
/// Builds the recipient list of the given push messages.
/// </summary>
public class AddRecipientsJobHandler(PushMessageJobService jobService)
    : IBackgroundJobHandler<AddRecipientsJobPayload>
{
    public virtual Task Execute(AddRecipientsJobPayload payload, IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return jobService.AddRecipientsJob(payload.MessageIds, cancellationToken);
    }
}
