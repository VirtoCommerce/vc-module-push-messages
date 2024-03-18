using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

public class PushMessageRecipientService : CrudService<PushMessageRecipient, PushMessageRecipientEntity, PushMessageRecipientChangingEvent, PushMessageRecipientChangedEvent>, IPushMessageRecipientService
{
    public PushMessageRecipientService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
    }

    protected override Task<IList<PushMessageRecipientEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IPushMessagesRepository)repository).GetRecipientsByIdsAsync(ids, responseGroup);
    }
}
