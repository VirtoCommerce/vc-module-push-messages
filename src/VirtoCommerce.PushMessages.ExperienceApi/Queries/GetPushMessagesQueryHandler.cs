using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQueryHandler : IQueryHandler<GetPushMessagesQuery, ExpPushMessagesResponse>
    {
        private readonly IPushMessageRecipientSearchService _recipientSearchService;

        public GetPushMessagesQueryHandler(IPushMessageRecipientSearchService recipientSearchService)
        {
            _recipientSearchService = recipientSearchService;
        }

        public async Task<ExpPushMessagesResponse> Handle(GetPushMessagesQuery request, CancellationToken cancellationToken)
        {
            var criteria = GetSearchCriteria(request);
            var searchResult = await _recipientSearchService.SearchNoCloneAsync(criteria);

            var result = AbstractTypeFactory<ExpPushMessagesResponse>.TryCreateInstance();
            result.Results = searchResult.Results.Select(x => ExpPushMessage.Create(x.Message, x)).ToList();
            result.TotalCount = searchResult.TotalCount;

            return result;
        }

        private static PushMessageRecipientSearchCriteria GetSearchCriteria(GetPushMessagesQuery request)
        {
            var criteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            criteria.UserId = request.UserId;
            criteria.IsRead = request.UnreadOnly ? false : null;
            criteria.WithHidden = request.WithHidden;
            criteria.Keyword = request.Keyword;
            criteria.Skip = request.Skip;
            criteria.Take = request.Take;
            criteria.ResponseGroup = PushMessageRecipientResponseGroup.WithMessages.ToString();

            return criteria;
        }
    }
}
