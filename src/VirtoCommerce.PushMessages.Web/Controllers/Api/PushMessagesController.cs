using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VirtoCommerce.PushMessages.Core;

namespace VirtoCommerce.PushMessages.Web.Controllers.Api
{
    [Route("api/push-messages")]
    public class PushMessagesController : Controller
    {
        // GET: api/push-messages
        /// <summary>
        /// Get message
        /// </summary>
        /// <remarks>Return "Hello world!" message</remarks>
        [HttpGet]
        [Route("")]
        [Authorize(ModuleConstants.Security.Permissions.Read)]
        public ActionResult<string> Get()
        {
            return Ok(new { result = "Hello world!" });
        }
    }
}
