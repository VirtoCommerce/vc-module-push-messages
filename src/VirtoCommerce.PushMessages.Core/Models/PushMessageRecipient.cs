using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageRecipient : AuditableEntity, ICloneable
{
    public string MessageId { get; set; }
    public string MemberId { get; set; }
    public string MemberName { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public bool IsRead { get; set; }
    public bool IsHidden { get; set; }

    public PushMessage Message { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
