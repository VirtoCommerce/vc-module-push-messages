using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Extensions;

public static class PushMessageRecipientChangedEventExtensions
{
    public static IDictionary<string, IList<PushMessageRecipient>> GetMessageIdsAndRecipients(this PushMessageRecipientChangedEvent @event)
    {
        return @event.ChangedEntries
            .Where(x => x.EntryState == EntryState.Added)
            .GroupBy(x => x.NewEntry.MessageId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.NewEntry).ToIList());
    }
}
