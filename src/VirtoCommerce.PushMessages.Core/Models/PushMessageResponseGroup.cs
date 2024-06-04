using System;

namespace VirtoCommerce.PushMessages.Core.Models;

[Flags]
public enum PushMessageResponseGroup
{
    None = 0,
    WithMembers = 1 << 0,
    WithReadRate = 1 << 1,
    Default = WithMembers,
}
