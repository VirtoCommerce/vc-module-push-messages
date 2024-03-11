using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class PushMessageChangedEvent : GenericChangedEntryEvent<PushMessage>
{
    public PushMessageChangedEvent(IEnumerable<GenericChangedEntry<PushMessage>> changedEntries)
        : base(changedEntries)
    {
    }
}
