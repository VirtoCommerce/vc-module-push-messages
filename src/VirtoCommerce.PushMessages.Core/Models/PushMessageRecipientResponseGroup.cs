using System;

namespace VirtoCommerce.PushMessages.Core.Models;

[Flags]
public enum PushMessageRecipientResponseGroup
{
    None = 0,
    WithMessages = 1 << 0,
    Full = WithMessages,
}
