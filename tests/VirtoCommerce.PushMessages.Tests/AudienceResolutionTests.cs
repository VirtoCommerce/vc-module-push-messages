using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Data.Services;
using Xunit;

namespace VirtoCommerce.PushMessages.Tests;

/// <summary>
/// VCST-5944 — the audience service resolves the same set of recipients the send job used to
/// build inline: organizations expanded, one recipient per security account, de-duplicated by
/// user id, members without an account dropped. Its counters explain how the final number was
/// reached, which is what the estimate panel renders.
/// </summary>
[Trait("Category", "Unit")]
public class AudienceResolutionTests
{
    [Fact]
    public async Task Organization_ExpandsToContacts_OneRecipientPerLogin_VCST5944()
    {
        // Arrange: one organization with seven contacts.
        // c1 holds two logins, c7 holds none.
        var contacts = Enumerable.Range(1, 7)
            .Select(i => NewContact($"c{i}", i == 1 ? 2 : i == 7 ? 0 : 1))
            .Cast<Member>()
            .ToList();

        var org = new Organization { Id = "org1", Name = "Acme" };

        var service = NewService(
            members: new Dictionary<string, Member> { ["org1"] = org },
            childrenByParentId: new Dictionary<string, IList<Member>> { ["org1"] = contacts });

        var criteria = new PushMessageAudienceCriteria
        {
            MemberIds = ["org1"],
            Take = int.MaxValue,
        };

        // Act
        var result = await service.ResolveAsync(criteria);

        // Assert: six contacts have an account, one of them twice.
        Assert.Equal(7, result.TotalCount);
        Assert.Equal(7, result.Results.Count);
        Assert.Equal(6, result.Results.Select(x => x.MemberId).Distinct().Count());
    }

    [Fact]
    public async Task ExcludedUserIds_AreNotReturned_VCST5944()
    {
        var contact = NewContact("c1", loginCount: 1);

        var service = NewService(members: new Dictionary<string, Member> { ["c1"] = contact });

        var criteria = new PushMessageAudienceCriteria
        {
            MemberIds = ["c1"],
            Take = int.MaxValue,
        };

        var result = await service.ResolveAsync(criteria, excludedUserIds: new HashSet<string> { "c1-user-1" });

        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task Counters_ExplainTheArithmetic_VCST5944()
    {
        // One organization and one standalone contact are matched by the query.
        // The organization expands into seven contacts; one holds two logins, one holds none.
        var orgContacts = Enumerable.Range(1, 7)
            .Select(i => NewContact($"c{i}", i == 1 ? 2 : i == 7 ? 0 : 1))
            .Cast<Member>()
            .ToList();

        var org = new Organization { Id = "org1", Name = "Acme" };
        var loner = NewContact("solo", loginCount: 1);

        var service = NewService(
            childrenByParentId: new Dictionary<string, IList<Member>> { ["org1"] = orgContacts },
            membersByKeyword: new Dictionary<string, IList<Member>> { ["role:Purchaser"] = [org, loner] });

        var criteria = new PushMessageAudienceCriteria
        {
            MemberQuery = "role:Purchaser",
            Take = int.MaxValue,
        };

        var result = await service.ResolveAsync(criteria);

        Assert.Equal(2, result.MembersMatched);        // org1 + solo
        Assert.Equal(1, result.CompaniesExpanded);     // org1 has no accounts
        Assert.Equal(7, result.PeopleFromCompanies);   // c1..c7
        Assert.Equal(7, result.PeopleInScope);         // solo + c1..c6; c7 has no account
        Assert.Equal(1, result.ExtraLogins);           // c1 holds two
        Assert.Equal(8, result.TotalCount);

        // The identity the estimate panel renders must hold.
        Assert.Equal(result.PeopleInScope + result.ExtraLogins, result.TotalCount);
    }

    [Fact]
    public async Task CompanyInsideCompany_IsExpandedButNotCountedAsAPerson_VCST5944()
    {
        // org1 holds c1 and the company org2; org2 holds c2 and c3.
        var org1 = new Organization { Id = "org1", Name = "Parent" };
        var org2 = new Organization { Id = "org2", Name = "Child" };

        var service = NewService(
            members: new Dictionary<string, Member> { ["org1"] = org1 },
            childrenByParentId: new Dictionary<string, IList<Member>>
            {
                ["org1"] = [NewContact("c1", loginCount: 1), org2],
                ["org2"] = [NewContact("c2", loginCount: 1), NewContact("c3", loginCount: 1)],
            });

        var result = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org1"], Take = 0 });

        Assert.Equal(2, result.CompaniesExpanded);
        Assert.Equal(3, result.PeopleFromCompanies);   // c1, c2, c3 — not org2
        Assert.Equal(3, result.PeopleInScope);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task PersonInTwoPickedCompanies_IsCountedOnce_VCST5944()
    {
        // Each company counted alone reaches two people; together they reach three, not four.
        var shared = NewContact("shared", loginCount: 1);

        var service = NewService(
            members: new Dictionary<string, Member>
            {
                ["org1"] = new Organization { Id = "org1", Name = "A" },
                ["org2"] = new Organization { Id = "org2", Name = "B" },
            },
            childrenByParentId: new Dictionary<string, IList<Member>>
            {
                ["org1"] = [NewContact("c1", loginCount: 1), shared],
                ["org2"] = [shared, NewContact("c2", loginCount: 1)],
            });

        var first = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org1"], Take = 0 });
        var second = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org2"], Take = 0 });
        var both = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org1", "org2"], Take = 0 });

        Assert.Equal(2, first.TotalCount);
        Assert.Equal(2, second.TotalCount);
        Assert.Equal(3, both.TotalCount);
    }

    [Fact]
    public async Task TakeZero_ReturnsCountersWithoutResults_VCST5944()
    {
        var contact = NewContact("c1", loginCount: 1);

        var service = NewService(members: new Dictionary<string, Member> { ["c1"] = contact });

        var result = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["c1"], Take = 0 });

        Assert.Equal(1, result.TotalCount);
        Assert.Empty(result.Results);
    }

    [Fact]
    public async Task TakeZero_CountsWithoutBuildingRecipients_VCST5944()
    {
        // The estimate asks for counters on every keystroke; building a recipient per login
        // would make that walk far more expensive than the counting it actually needs.
        var contacts = Enumerable.Range(1, 7)
            .Select(i => NewContact($"c{i}", i == 1 ? 2 : i == 7 ? 0 : 1))
            .Cast<Member>()
            .ToList();

        var org = new Organization { Id = "org1", Name = "Acme" };

        var service = NewService(
            members: new Dictionary<string, Member> { ["org1"] = org },
            childrenByParentId: new Dictionary<string, IList<Member>> { ["org1"] = contacts });

        var counted = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org1"], Take = 0 });
        var listed = await service.ResolveAsync(new PushMessageAudienceCriteria { MemberIds = ["org1"], Take = int.MaxValue });

        Assert.Empty(counted.Results);
        Assert.Equal(listed.TotalCount, counted.TotalCount);
        Assert.Equal(listed.PeopleInScope, counted.PeopleInScope);
        Assert.Equal(listed.ExtraLogins, counted.ExtraLogins);
    }

    private static PushMessageAudienceService NewService(
        IDictionary<string, Member> members = null,
        IDictionary<string, IList<Member>> childrenByParentId = null,
        IDictionary<string, IList<Member>> membersByKeyword = null)
    {
        return new PushMessageAudienceService(
            new FakeSettingsManager(),
            new FakeMemberService(members ?? new Dictionary<string, Member>()),
            new FakeMemberSearchService(childrenByParentId, membersByKeyword));
    }

    private static Contact NewContact(string id, int loginCount)
    {
        return new Contact
        {
            Id = id,
            Name = id,
            SecurityAccounts = Enumerable.Range(1, loginCount)
                .Select(i => new ApplicationUser { Id = $"{id}-user-{i}", UserName = $"{id}-login-{i}" })
                .ToList(),
        };
    }
}
