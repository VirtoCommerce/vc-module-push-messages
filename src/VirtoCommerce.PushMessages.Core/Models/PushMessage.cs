using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessage : AuditableEntity, ICloneable
{
    public string Topic { get; set; }

    public string ShortMessage { get; set; }

    public DateTime? StartDate { get; set; }

    public string Status { get; set; } = PushMessageStatus.Draft;

    public bool TrackNewRecipients { get; set; }

    public string MemberQuery { get; set; }

    public IList<string> MemberIds { get; set; }

    public virtual object Clone()
    {
        return MemberwiseClone();
    }

    public virtual bool HasRecipients()
    {
        return MemberIds != null && MemberIds.Count > 0 || !string.IsNullOrEmpty(MemberQuery);
    }
}
