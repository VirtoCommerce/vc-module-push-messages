using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Handlers;

public class PushMessageChangedEventHandler : IEventHandler<PushMessageChangedEvent>
{
    private readonly IPushMessageJobService _pushMessageJobService;

    public PushMessageChangedEventHandler(IPushMessageJobService pushMessageJobService)
    {
        _pushMessageJobService = pushMessageJobService;
    }

    public Task Handle(PushMessageChangedEvent message)
    {
        // Add recipients if:
        // - new message with status Sent
        // - modified message with status changed to Sent
        // - modified message with status Sent and TrackNewRecipients changed to true

        var messageIds = message.ChangedEntries
            .Where(x =>
                x.NewEntry.Status == PushMessageStatus.Sent && x.NewEntry.HasRecipients() && (
                    x.EntryState == EntryState.Added ||
                    x.EntryState == EntryState.Modified && x.OldEntry.Status != PushMessageStatus.Sent ||
                    x.EntryState == EntryState.Modified && x.NewEntry.TrackNewRecipients && !x.OldEntry.TrackNewRecipients))
            .Select(x => x.NewEntry.Id)
            .ToList();

        if (messageIds.Count > 0)
        {
            _pushMessageJobService.EnqueueAddRecipients(messageIds);
        }

        return Task.CompletedTask;
    }
}
