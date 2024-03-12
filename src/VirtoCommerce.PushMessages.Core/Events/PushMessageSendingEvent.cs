using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class PushMessageSendingEvent : GenericChangedEntryEvent<PushMessageCombined>
{
    public PushMessageSendingEvent(IEnumerable<GenericChangedEntry<PushMessageCombined>> changedEntries)
        : base(changedEntries)
    {
    }
}
