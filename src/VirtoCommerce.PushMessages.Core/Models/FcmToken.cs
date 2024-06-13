using System;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class FcmToken : AuditableEntity, ICloneable
{
    public string Token { get; set; }

    public string UserId { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
