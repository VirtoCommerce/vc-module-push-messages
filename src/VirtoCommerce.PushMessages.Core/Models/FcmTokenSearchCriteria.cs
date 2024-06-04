using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class FcmTokenSearchCriteria : SearchCriteriaBase
{
    public string Token { get; set; }

    public IList<string> UserIds { get; set; }
}
