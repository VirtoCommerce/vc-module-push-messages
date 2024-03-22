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

public class PushMessageRecipientSearchService : SearchService<PushMessageRecipientSearchCriteria, PushMessageRecipientSearchResult, PushMessageRecipient, PushMessageRecipientEntity>, IPushMessageRecipientSearchService
{
    public PushMessageRecipientSearchService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IPushMessageRecipientService crudService,
        IOptions<CrudOptions> crudOptions)
        : base(repositoryFactory, platformMemoryCache, crudService, crudOptions)
    {
    }

    protected override IQueryable<PushMessageRecipientEntity> BuildQuery(IRepository repository, PushMessageRecipientSearchCriteria criteria)
    {
        var query = ((IPushMessagesRepository)repository).Recipients;

        if (!criteria.WithHidden)
        {
            query = query.Where(x => !x.IsHidden);
        }

        if (criteria.MessageId != null)
        {
            query = query.Where(x => x.MessageId == criteria.MessageId);
        }

        if (criteria.UserId != null)
        {
            query = query.Where(x => x.UserId == criteria.UserId);
        }

        if (criteria.IsRead != null)
        {
            query = query.Where(x => x.IsRead == criteria.IsRead.Value);
        }

        if (!string.IsNullOrEmpty(criteria.Keyword))
        {
            query = query.Where(x => x.MemberName.Contains(criteria.Keyword) ||
                                     x.UserName.Contains(criteria.Keyword) ||
                                     x.MemberId.Contains(criteria.Keyword) ||
                                     x.UserId.Contains(criteria.Keyword));
        }

        return query;
    }

    protected override IList<SortInfo> BuildSortExpression(PushMessageRecipientSearchCriteria criteria)
    {
        var sortInfos = criteria.SortInfos;

        if (sortInfos.IsNullOrEmpty())
        {
            sortInfos =
            [
                new SortInfo { SortColumn = nameof(PushMessageEntity.CreatedDate), SortDirection = SortDirection.Descending},
                new SortInfo { SortColumn = nameof(PushMessageRecipientEntity.Id) },
            ];
        }

        return sortInfos;
    }
}
