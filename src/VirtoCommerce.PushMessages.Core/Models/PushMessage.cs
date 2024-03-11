using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessage : AuditableEntity, ICloneable
{
    public string ShortMessage { get; set; }

    public IList<string> MemberIds { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
