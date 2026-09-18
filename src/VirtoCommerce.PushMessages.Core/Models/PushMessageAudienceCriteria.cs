using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageAudienceCriteria
{
    public string MemberQuery { get; set; }
    public IList<string> MemberIds { get; set; }
    public int Skip { get; set; }

    /// <summary>Size of the returned page. 0 returns counters without any results.</summary>
    public int Take { get; set; }
}
