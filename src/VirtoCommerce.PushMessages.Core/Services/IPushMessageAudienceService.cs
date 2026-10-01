using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageAudienceService
{
    /// <param name="messageId">Stamped onto every recipient. Null when previewing.</param>
    Task<PushMessageAudienceResult> ResolveAsync(
        PushMessageAudienceCriteria criteria,
        string messageId = null,
        ISet<string> excludedUserIds = null);
}
