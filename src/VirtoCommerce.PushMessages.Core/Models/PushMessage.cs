using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessage : AuditableEntity, ICloneable
{
    public string ShortMessage { get; set; }

    public IList<string> MemberIds { get; set; }

    [JsonIgnore]
    public IList<string> UserIds { get; set; }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
