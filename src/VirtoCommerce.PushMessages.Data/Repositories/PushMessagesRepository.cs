using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PushMessages.Data.Models;

namespace VirtoCommerce.PushMessages.Data.Repositories;

public class PushMessagesRepository : DbContextRepositoryBase<PushMessagesDbContext>, IPushMessagesRepository
{
    public PushMessagesRepository(PushMessagesDbContext dbContext, IUnitOfWork unitOfWork = null)
        : base(dbContext, unitOfWork)
    {
    }

    public IQueryable<PushMessageEntity> PushMessages => DbContext.Set<PushMessageEntity>();

    public virtual async Task<IList<PushMessageEntity>> GetPushMessagesByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        return ids.Count == 1
            ? await PushMessages.Where(x => x.Id == ids.First()).ToListAsync()
            : await PushMessages.Where(x => ids.Contains(x.Id)).ToListAsync();
    }
}
