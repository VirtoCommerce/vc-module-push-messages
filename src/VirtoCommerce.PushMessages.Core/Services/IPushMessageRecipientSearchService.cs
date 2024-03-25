using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageRecipientSearchService : ISearchService<PushMessageRecipientSearchCriteria, PushMessageRecipientSearchResult, PushMessageRecipient>
{
}
