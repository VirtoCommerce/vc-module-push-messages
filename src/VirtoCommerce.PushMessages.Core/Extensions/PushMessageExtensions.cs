using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Extensions;

public static class PushMessageExtensions
{
    public static bool IsSent(this GenericChangedEntry<PushMessage> entry)
    {
        return entry.NewEntry.Status == PushMessageStatus.Sent &&
               (entry.EntryState == EntryState.Added ||
                entry.EntryState == EntryState.Modified && entry.OldEntry.Status != PushMessageStatus.Sent);
    }
}
