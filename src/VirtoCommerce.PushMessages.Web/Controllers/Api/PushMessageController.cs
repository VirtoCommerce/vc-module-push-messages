using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.Web.Controllers.Api;

[Route("api/push-message")]
public class PushMessageController : Controller
{
    private readonly IPushMessageService _crudService;
    private readonly IPushMessageSearchService _searchService;

    public PushMessageController(
        IPushMessageService crudService,
        IPushMessageSearchService searchService)
    {
        _crudService = crudService;
        _searchService = searchService;
    }

    [HttpPost("search-recipients")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessageRecipientSearchResult>> SearchRecipients([FromBody] PushMessageRecipientSearchCriteria criteria)
    {
        await Task.CompletedTask;
        var result = new PushMessageRecipientSearchResult
        {
            Results =
            [
                new PushMessageRecipient { MessageId = criteria.MessageId, UserId = "1", IsRead = false, },
                new PushMessageRecipient { MessageId = criteria.MessageId, UserId = "2", IsRead = true, },
            ],
            TotalCount = 2,
        };

        return Ok(result);
    }

    [HttpPost("search")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessageSearchResult>> Search([FromBody] PushMessageSearchCriteria criteria)
    {
        var result = await _searchService.SearchAsync(criteria);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(ModuleConstants.Security.Permissions.Create)]
    public async Task<ActionResult<PushMessage>> Create([FromBody] PushMessage model)
    {
        model.Id = null;
        await _crudService.SaveChangesAsync([model]);
        return Ok(model);
    }

    [HttpGet("{id}")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessage>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        var retVal = await _crudService.GetNoCloneAsync(id, responseGroup);
        return Ok(retVal);
    }

    [HttpDelete]
    [Authorize(ModuleConstants.Security.Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await _crudService.DeleteAsync(ids);
        return NoContent();
    }
}
