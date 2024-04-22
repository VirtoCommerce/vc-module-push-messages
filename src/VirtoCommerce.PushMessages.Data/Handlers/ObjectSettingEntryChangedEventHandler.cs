using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings.Events;
using VirtoCommerce.PushMessages.Core.BackgroundJobs;
using JobSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.BackgroundJobs;

namespace VirtoCommerce.PushMessages.Data.Handlers;

public class ObjectSettingEntryChangedEventHandler : IEventHandler<ObjectSettingChangedEvent>
{
    private readonly IPushMessageJobService _pushMessageJobService;

    public ObjectSettingEntryChangedEventHandler(IPushMessageJobService pushMessageJobService)
    {
        _pushMessageJobService = pushMessageJobService;
    }

    public virtual async Task Handle(ObjectSettingChangedEvent message)
    {
        if (message.ChangedEntries.Any(x => x.EntryState is EntryState.Modified or EntryState.Added &&
                                            (x.NewEntry.Name == JobSettings.Enable.Name ||
                                             x.NewEntry.Name == JobSettings.CronExpression.Name)))
        {
            await _pushMessageJobService.StartStopRecurringJobs();
        }
    }
}
