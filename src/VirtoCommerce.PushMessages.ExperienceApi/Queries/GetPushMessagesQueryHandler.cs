using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
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
            var result = AbstractTypeFactory<ExpPushMessagesResponse>.TryCreateInstance();

            var criteria = GetSearchCriteria(request);

            var searchResult = await _recipientSearchService.SearchAsync(criteria);

            result.Items = GetExpSearchMessages(searchResult.Results);
            result.TotalCount = searchResult.TotalCount;
            result.UnreadCount = result.Items.Count(x => !x.IsRead);

            return result;
        }

        private List<ExpPushMessage> GetExpSearchMessages(IList<PushMessageRecipient> messages)
        {
            return messages
                .Select(x =>
                    new ExpPushMessage
                    {
                        Id = x.Message.Id,
                        ShortMessage = x.Message.ShortMessage,
                        CreatedDate = x.Message.CreatedDate,
                        UserId = x.UserId,
                        IsRead = x.IsRead,
                    })
                .ToList();
        }

        private static PushMessageRecipientSearchCriteria GetSearchCriteria(GetPushMessagesQuery request)
        {
            var criteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            criteria.UserId = request.UserId;
            criteria.WithHidden = request.WithHidden;
            criteria.IsRead = request.UnreadOnly ? false : null;
            criteria.Keyword = request.Keyword;
            criteria.Take = request.Take;
            criteria.Skip = request.Skip;
            criteria.ResponseGroup = PushMessageRecipientResponseGroup.WithMessages.ToString();

            return criteria;
        }
    }
}
