using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageRecipientSearchCriteria : SearchCriteriaBase
{
    public string MessageId { get; set; }
    public string UserId { get; set; }
    public bool? IsRead { get; set; }
    public bool WithHidden { get; set; }
}
