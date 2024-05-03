using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageSearchCriteria : SearchCriteriaBase
{
    public bool? IsDraft { get; set; }
    public bool? TrackNewRecipients { get; set; }
    public DateTime? StartDateBefore { get; set; }
    public IList<string> Statuses { get; set; }
}
