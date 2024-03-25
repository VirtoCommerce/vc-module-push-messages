using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageMember : AuditableEntity
{
    public string MemberId { get; set; }
    public string MemberName { get; set; }
    public string MemberType { get; set; }
}
