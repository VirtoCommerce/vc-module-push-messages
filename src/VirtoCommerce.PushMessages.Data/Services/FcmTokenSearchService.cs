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

public class FcmTokenSearchService : SearchService<FcmTokenSearchCriteria, FcmTokenSearchResult, FcmToken, FcmTokenEntity>, IFcmTokenSearchService
{
    public FcmTokenSearchService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IFcmTokenService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<FcmTokenEntity> BuildQuery(IRepository repository, FcmTokenSearchCriteria criteria)
    {
        var query = ((IPushMessagesRepository)repository).FcmTokens;

        if (criteria.Token != null)
        {
            query = query.Where(x => x.Token == criteria.Token);
        }

        if (!criteria.UserIds.IsNullOrEmpty())
        {
            query = criteria.UserIds.Count == 1
                ? query.Where(x => x.UserId == criteria.UserIds.First())
                : query.Where(x => criteria.UserIds.Contains(x.UserId));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(FcmTokenSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(FcmTokenEntity.CreatedDate), SortDirection = SortDirection.Descending },
                new SortInfo { SortColumn = nameof(FcmTokenEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
