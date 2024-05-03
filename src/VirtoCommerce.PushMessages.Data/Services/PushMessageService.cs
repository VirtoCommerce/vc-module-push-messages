using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Models;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageService : CrudService<PushMessage, PushMessageEntity, PushMessageChangingEvent, PushMessageChangedEvent>, IPushMessageService
{
    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    public virtual async Task<PushMessage> ChangeTracking(string messageId, bool value)
    {
        var message = await this.GetByIdAsync(messageId);

        if (message != null && message.TrackNewRecipients != value)
        {
            message.TrackNewRecipients = value;

            // Skip status validation
            await base.SaveChangesAsync([message]);
        }

        return message;
    }

    public override async Task SaveChangesAsync(IList<PushMessage> models)
    {
        var ids = models.Where(x => x.Id != null).Select(x => x.Id).ToList();
        await ValidateStatusAsync(ids);
        await base.SaveChangesAsync(models);
    }

    public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        await ValidateStatusAsync(ids);
        await base.DeleteAsync(ids, softDelete);
    }


    protected override Task<IList<PushMessageEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IPushMessagesRepository)repository).GetMessagesByIdsAsync(ids, responseGroup);
    }

    protected override void ClearCache(IList<PushMessage> models)
    {
        base.ClearCache(models);

        GenericCachingRegion<PushMessageRecipient>.ExpireRegion();
        GenericSearchCachingRegion<PushMessageRecipient>.ExpireRegion();
    }


    private async Task ValidateStatusAsync(IList<string> ids)
    {
        var models = await GetAsync(ids);

        if (models.Any(x => x.Status == PushMessageStatus.Sent))
        {
            throw new InvalidOperationException($"Cannot modify or delete messages with status {PushMessageStatus.Sent}.");
        }
    }
}
