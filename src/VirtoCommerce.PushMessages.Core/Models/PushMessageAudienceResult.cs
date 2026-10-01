using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageAudienceResult
{
    public int TotalCount { get; set; }

    /// <summary>Distinct members seeded from MemberIds and MemberQuery, before any expansion.</summary>
    public int MembersMatched { get; set; }

    /// <summary>Members with no security accounts that were replaced by their children.</summary>
    public int CompaniesExpanded { get; set; }

    /// <summary>Distinct members queued as a result of those expansions.</summary>
    public int PeopleFromCompanies { get; set; }

    /// <summary>Distinct members that produced at least one recipient.</summary>
    public int PeopleInScope { get; set; }

    /// <summary>Recipients beyond the first for members holding more than one login.</summary>
    public int ExtraLogins { get; set; }

    public IList<PushMessageRecipient> Results { get; set; } = [];
}
