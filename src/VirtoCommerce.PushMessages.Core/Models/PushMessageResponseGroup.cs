using System;

namespace VirtoCommerce.PushMessages.Core.Models;

[Flags]
public enum PushMessageResponseGroup
{
    None = 0,
    WithMembers = 1 << 0,
    Full = WithMembers,
}
