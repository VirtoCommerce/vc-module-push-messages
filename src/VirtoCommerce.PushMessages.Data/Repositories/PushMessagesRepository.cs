using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Data.Models;

namespace VirtoCommerce.PushMessages.Data.Repositories;

public class PushMessagesRepository : DbContextRepositoryBase<PushMessagesDbContext>, IPushMessagesRepository
{
    public PushMessagesRepository(PushMessagesDbContext dbContext, IUnitOfWork unitOfWork = null)
        : base(dbContext, unitOfWork)
    {
    }

    public IQueryable<PushMessageEntity> Messages => DbContext.Set<PushMessageEntity>();

    public IQueryable<PushMessageMemberEntity> Members => DbContext.Set<PushMessageMemberEntity>();

    public IQueryable<PushMessageRecipientEntity> Recipients => DbContext.Set<PushMessageRecipientEntity>();

    public virtual async Task<IList<PushMessageEntity>> GetMessagesByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        var messages = ids.Count == 1
            ? await Messages.Where(x => x.Id == ids.First()).ToListAsync()
            : await Messages.Where(x => ids.Contains(x.Id)).ToListAsync();

        if (messages.Count > 0)
        {
            var responseGroupEnum = EnumUtility.SafeParseFlags(responseGroup, PushMessageResponseGroup.Default);

            if (responseGroupEnum.HasFlag(PushMessageResponseGroup.WithMembers))
            {
                var messageIds = messages.Select(x => x.Id).ToList();
                await Members.Where(x => messageIds.Contains(x.MessageId)).LoadAsync();
            }
        }

        return messages;
    }

    public virtual async Task<IList<PushMessageRecipientEntity>> GetRecipientsByIdsAsync(IList<string> ids, string responseGroup)
    {
        if (ids.IsNullOrEmpty())
        {
            return [];
        }

        var recipients = ids.Count == 1
            ? await Recipients.Where(x => x.Id == ids.First()).ToListAsync()
            : await Recipients.Where(x => ids.Contains(x.Id)).ToListAsync();

        if (recipients.Count > 0)
        {
            var responseGroupEnum = EnumUtility.SafeParseFlags(responseGroup, PushMessageRecipientResponseGroup.None);

            if (responseGroupEnum.HasFlag(PushMessageRecipientResponseGroup.WithMessages))
            {
                var messageIds = recipients.Select(x => x.MessageId).ToList();
                await Messages.Where(x => messageIds.Contains(x.Id)).LoadAsync();
            }
        }

        return recipients;
    }
}
