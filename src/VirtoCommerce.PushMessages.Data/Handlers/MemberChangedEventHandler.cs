using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Events;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.Handlers;

public class MemberChangedEventHandler : IEventHandler<MemberChangedEvent>
{
    private readonly IPushMessageJobService _pushMessageJobService;

    public MemberChangedEventHandler(IPushMessageJobService pushMessageJobService)
    {
        _pushMessageJobService = pushMessageJobService;
    }

    public Task Handle(MemberChangedEvent message)
    {
        if (message.ChangedEntries.Any(x => x.EntryState is EntryState.Added or EntryState.Modified))
        {
            _pushMessageJobService.EnqueueAddRecipients();
        }

        return Task.CompletedTask;
    }
}
