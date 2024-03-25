using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class PushMessageRecipientChangingEvent : GenericChangedEntryEvent<PushMessageRecipient>
{
    public PushMessageRecipientChangingEvent(IEnumerable<GenericChangedEntry<PushMessageRecipient>> changedEntries)
        : base(changedEntries)
    {
    }
}
