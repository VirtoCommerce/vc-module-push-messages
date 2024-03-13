using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessage : AuditableEntity, ICloneable
{
    public string ShortMessage { get; set; }

    // Temporary property. Remove after switching to multiselect in the UI
    public string MemberId { get; set; }

    public IList<string> MemberIds { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
