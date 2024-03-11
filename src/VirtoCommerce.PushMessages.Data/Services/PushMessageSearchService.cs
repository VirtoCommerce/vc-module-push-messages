using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Models;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageSearchService : SearchService<PushMessageSearchCriteria, PushMessageSearchResult, PushMessage, PushMessageEntity>, IPushMessageSearchService
{
    public PushMessageSearchService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IPushMessageService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<PushMessageEntity> BuildQuery(IRepository repository, PushMessageSearchCriteria criteria)
    {
        var query = ((IPushMessagesRepository)repository).Messages;

        if (criteria.UserId != null)
        {
            query = criteria.IsRead == null
                ? query.Where(x => x.Recipients.Any(r => r.UserId == criteria.UserId))
                : query.Where(x => x.Recipients.Any(r => r.UserId == criteria.UserId && r.IsRead == criteria.IsRead.Value));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(PushMessageSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(PushMessageEntity.CreatedDate), SortDirection = SortDirection.Descending},
                new SortInfo { SortColumn = nameof(PushMessageEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
