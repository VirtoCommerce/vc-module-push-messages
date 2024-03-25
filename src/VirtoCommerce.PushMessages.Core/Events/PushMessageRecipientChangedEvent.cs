using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class PushMessageRecipientChangedEvent : GenericChangedEntryEvent<PushMessageRecipient>
{
    public PushMessageRecipientChangedEvent(IEnumerable<GenericChangedEntry<PushMessageRecipient>> changedEntries)
        : base(changedEntries)
    {
    }
}
