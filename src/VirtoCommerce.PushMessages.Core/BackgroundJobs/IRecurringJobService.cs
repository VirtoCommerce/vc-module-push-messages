using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Settings.Events;

namespace VirtoCommerce.PushMessages.Core.BackgroundJobs;

public interface IRecurringJobService : IEventHandler<ObjectSettingChangedEvent>
{
    Task StartStopRecurringJobs();
}
