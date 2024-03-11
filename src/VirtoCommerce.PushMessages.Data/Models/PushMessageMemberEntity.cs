using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Data.Models;

public class PushMessageMemberEntity : AuditableEntity
{
    [StringLength(128)]
    public string MessageId { get; set; }

    [StringLength(128)]
    public string MemberId { get; set; }

    public PushMessageEntity Message { get; set; }
}
