using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageAudienceResult
{
    public int TotalCount { get; set; }

    /// <summary>People reached from MemberIds and MemberQuery, counted before de-duplication.</summary>
    public int MatchedPeople { get; set; }

    /// <summary>People reached by expanding companies, counted before de-duplication.</summary>
    public int FoundInCompanies { get; set; }

    /// <summary>MatchedPeople + FoundInCompanies - PeopleInScope: people reached more than once.</summary>
    public int Overlaps { get; set; }

    /// <summary>Distinct members that produced at least one recipient.</summary>
    public int PeopleInScope { get; set; }

    /// <summary>Recipients beyond the first for members holding more than one login.</summary>
    public int ExtraLogins { get; set; }

    /// <summary>MemberIds entries that are people.</summary>
    public int PickedPeople { get; set; }

    /// <summary>MemberIds entries that are companies.</summary>
    public int PickedCompanies { get; set; }

    public IList<PushMessageRecipient> Results { get; set; } = [];
}
