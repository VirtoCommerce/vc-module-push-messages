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

    [HttpPost("search")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessageSearchResult>> Search([FromBody] PushMessageSearchCriteria criteria)
    {
        var result = await _searchService.SearchAsync(criteria);

        return Ok(result);
    }

    [HttpPost]
    [Authorize(ModuleConstants.Security.Permissions.Create)]
    public Task<ActionResult<PushMessage>> Create([FromBody] PushMessage model)
    {
        model.Id = null;
        return Update(model);
    }

    [HttpPut]
    [Authorize(ModuleConstants.Security.Permissions.Update)]
    public async Task<ActionResult<PushMessage>> Update([FromBody] PushMessage model)
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
