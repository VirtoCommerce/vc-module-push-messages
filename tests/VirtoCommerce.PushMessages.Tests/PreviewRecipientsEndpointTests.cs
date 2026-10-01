using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Web.Controllers.Api;
using Xunit;

namespace VirtoCommerce.PushMessages.Tests;

/// <summary>
/// VCST-5944 — the preview endpoint hands the caller's audience definition to the same service
/// the send job uses, and returns its counters untouched. Anything that quietly rewrites the
/// criteria here would make the previewed number differ from the sent one.
/// </summary>
[Trait("Category", "Unit")]
public class PreviewRecipientsEndpointTests
{
    [Fact]
    public async Task PreviewRecipients_PassesCriteriaThrough_AndReturnsCounters_VCST5944()
    {
        var resolved = new PushMessageAudienceResult
        {
            TotalCount = 15,
            MembersMatched = 8,
            CompaniesExpanded = 1,
            PeopleFromCompanies = 6,
            PeopleInScope = 13,
            ExtraLogins = 2,
        };

        var audienceService = new RecordingAudienceService(resolved);
        var controller = new PushMessageController(null, null, null, audienceService);

        var criteria = new PushMessageAudienceCriteria
        {
            MemberQuery = "role:Purchaser",
            MemberIds = ["klimaat-de-boer"],
            Take = 0,
        };

        var response = await controller.PreviewRecipients(criteria);

        Assert.Same(criteria, audienceService.LastCriteria);

        var ok = Assert.IsType<OkObjectResult>(response.Result);
        Assert.Same(resolved, ok.Value);
    }
}
