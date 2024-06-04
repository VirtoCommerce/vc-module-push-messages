using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class FcmTokenChangedEvent : GenericChangedEntryEvent<FcmToken>
{
    public FcmTokenChangedEvent(IEnumerable<GenericChangedEntry<FcmToken>> changedEntries)
        : base(changedEntries)
    {
    }
}
