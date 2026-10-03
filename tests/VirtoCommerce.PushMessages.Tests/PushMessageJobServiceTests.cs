using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using VirtoCommerce.Platform.Core.Jobs;
using VirtoCommerce.PushMessages.Data.BackgroundJobs;
using Xunit;

namespace VirtoCommerce.PushMessages.Tests;

public class PushMessageJobServiceTests
{
    private readonly RecordingBackgroundJob _backgroundJob = new();
    private readonly PushMessageJobService _jobService;

    public PushMessageJobServiceTests()
    {
        BackgroundJob.Initialize(new ServiceCollection().AddScoped<IBackgroundJob>(_ => _backgroundJob).BuildServiceProvider());

        // EnqueueAddRecipients only enqueues, so none of the services it would use while running is needed here.
        _jobService = new PushMessageJobService(null, null, null, null, null, null, null);
    }

    [Fact]
    public void EnqueueAddRecipients_WithMessageIds_EnqueuesAddRecipientsJobForThoseMessages()
    {
        _jobService.EnqueueAddRecipients(["message-1", "message-2"]);

        var (handlerType, payload) = Assert.Single(_backgroundJob.Enqueued);
        Assert.Equal(typeof(AddRecipientsJobHandler), handlerType);
        Assert.Equal(["message-1", "message-2"], Assert.IsType<AddRecipientsJobPayload>(payload).MessageIds);
    }

    [Fact]
    public void EnqueueAddRecipients_WithoutMessageIds_EnqueuesTrackNewRecipientsJob()
    {
        _jobService.EnqueueAddRecipients();

        var (handlerType, payload) = Assert.Single(_backgroundJob.Enqueued);
        Assert.Equal(typeof(TrackNewRecipientsJobHandler), handlerType);
        Assert.IsType<TrackNewRecipientsJobPayload>(payload);
    }

    private sealed class RecordingBackgroundJob : IBackgroundJob
    {
        public List<(Type HandlerType, object Payload)> Enqueued { get; } = [];

        public Task<string> Enqueue<THandler>(object payload, EnqueueOptions options = null, CancellationToken cancellationToken = default)
            where THandler : class
        {
            Enqueued.Add((typeof(THandler), payload));
            return Task.FromResult("job-id");
        }
    }
}
