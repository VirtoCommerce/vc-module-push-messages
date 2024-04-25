using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageSearchCriteria : SearchCriteriaBase
{
    public DateTime? StartDateBefore { get; set; }
    public IList<string> Statuses { get; set; }
}
