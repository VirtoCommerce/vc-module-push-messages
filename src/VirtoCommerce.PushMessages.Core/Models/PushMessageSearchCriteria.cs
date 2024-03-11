using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageSearchCriteria : SearchCriteriaBase
{
    public string UserId { get; set; }
    public bool? IsRead { get; set; }
}
