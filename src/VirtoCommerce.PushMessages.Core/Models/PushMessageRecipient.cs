using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageRecipient : AuditableEntity
{
    public string MessageId { get; set; }
    public string UserId { get; set; }
    public bool IsRead { get; set; }
}
