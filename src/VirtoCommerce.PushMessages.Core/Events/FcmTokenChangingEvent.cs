using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Events;

public class FcmTokenChangingEvent : GenericChangedEntryEvent<FcmToken>
{
    public FcmTokenChangingEvent(IEnumerable<GenericChangedEntry<FcmToken>> changedEntries)
        : base(changedEntries)
    {
    }
}
