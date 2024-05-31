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
    private readonly IPushMessageService _messageService;
    private readonly IPushMessageSearchService _messageSearchService;
    private readonly IPushMessageRecipientSearchService _recipientSearchService;

    public PushMessageController(
        IPushMessageService messageService,
        IPushMessageSearchService messageSearchService,
        IPushMessageRecipientSearchService recipientSearchService)
    {
        _messageService = messageService;
        _messageSearchService = messageSearchService;
        _recipientSearchService = recipientSearchService;
    }

    [HttpPost("search-recipients")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessageRecipientSearchResult>> SearchRecipients([FromBody] PushMessageRecipientSearchCriteria criteria)
    {
        var result = await _recipientSearchService.SearchNoCloneAsync(criteria);
        return Ok(result);
    }

    [HttpPost("search")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessageSearchResult>> Search([FromBody] PushMessageSearchCriteria criteria)
    {
        var result = await _messageSearchService.SearchNoCloneAsync(criteria);
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
        await _messageService.SaveChangesAsync([model]);
        return Ok(await _messageService.GetNoCloneAsync(model.Id));
    }

    [HttpPut("{id}/tracking/{value}")]
    [Authorize(ModuleConstants.Security.Permissions.Update)]
    public async Task<ActionResult<PushMessage>> ChangeTracking([FromRoute] string id, [FromRoute] bool value)
    {
        await _messageService.ChangeTracking(id, value);
        return Ok(await _messageService.GetNoCloneAsync(id));
    }

    [HttpGet("{id}")]
    [Authorize(ModuleConstants.Security.Permissions.Read)]
    public async Task<ActionResult<PushMessage>> Get([FromRoute] string id, [FromQuery] string responseGroup = null)
    {
        return Ok(await _messageService.GetNoCloneAsync(id, responseGroup));
    }

    [HttpDelete]
    [Authorize(ModuleConstants.Security.Permissions.Delete)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Delete([FromQuery] string[] ids)
    {
        await _messageService.DeleteAsync(ids);
        return NoContent();
    }
}
