using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IFcmTokenSearchService : ISearchService<FcmTokenSearchCriteria, FcmTokenSearchResult, FcmToken>
{
}
