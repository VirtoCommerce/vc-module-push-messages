using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class PushMessageChangingEvent : GenericChangedEntryEvent<PushMessage>
{
    public PushMessageChangingEvent(IEnumerable<GenericChangedEntry<PushMessage>> changedEntries)
        : base(changedEntries)
    {
    }
}
