using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageSearchService : ISearchService<PushMessageSearchCriteria, PushMessageSearchResult, PushMessage>
{
}
