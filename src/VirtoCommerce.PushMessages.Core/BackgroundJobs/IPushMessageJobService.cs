using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.BackgroundJobs;

public interface IPushMessageJobService
{
    void EnqueueAddRecipients(IList<string> messageIds = null);
}
