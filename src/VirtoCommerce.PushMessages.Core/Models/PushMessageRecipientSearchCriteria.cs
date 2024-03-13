using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageRecipientSearchCriteria : SearchCriteriaBase
{
    public string MessageId { get; set; }
}
