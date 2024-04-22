using System.Threading.Tasks;

namespace VirtoCommerce.PushMessages.Core.BackgroundJobs;

public interface IPushMessageJobService
{
    Task StartStopRecurringJobs();
}
