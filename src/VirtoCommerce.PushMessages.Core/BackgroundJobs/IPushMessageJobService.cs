using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.BackgroundJobs;

public interface IPushMessageJobService : IRecurringJobService
{
    void EnqueueAddRecipients(IList<string> messageIds = null);
}
