using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Data.Models;

namespace VirtoCommerce.PushMessages.Data.Repositories;

public interface IPushMessagesRepository : IRepository
{
    public IQueryable<PushMessageEntity> PushMessages { get; }
    Task<IList<PushMessageEntity>> GetPushMessagesByIdsAsync(IList<string> ids, string responseGroup);
}
