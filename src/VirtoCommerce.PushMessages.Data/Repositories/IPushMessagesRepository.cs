using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Data.Models;

namespace VirtoCommerce.PushMessages.Data.Repositories;

public interface IPushMessagesRepository : IRepository
{
    public IQueryable<PushMessageEntity> Messages { get; }

    public IQueryable<PushMessageMemberEntity> Members { get; }

    public IQueryable<PushMessageRecipientEntity> Recipients { get; }

    Task<IList<PushMessageEntity>> GetMessagesByIdsAsync(IList<string> ids, string responseGroup);

    Task<IList<PushMessageRecipientEntity>> GetRecipientsByIdsAsync(IList<string> ids, string responseGroup);
}
