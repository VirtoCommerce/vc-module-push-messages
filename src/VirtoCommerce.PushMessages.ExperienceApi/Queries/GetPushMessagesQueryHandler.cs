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
        private readonly IPushMessageSearchService _pushMessageSearchService;
        private readonly IPushMessageService _pushMessageService;

        public GetPushMessagesQueryHandler(IPushMessageSearchService pushMessageSearchService, IPushMessageService pushMessageService)
        {
            _pushMessageSearchService = pushMessageSearchService;
            _pushMessageService = pushMessageService;
        }

        public async Task<ExpPushMessagesResponse> Handle(GetPushMessagesQuery request, CancellationToken cancellationToken)
        {
            var pushMessages = new List<PushMessage>();

            var skip = 0;
            var take = 50;
            PushMessageSearchResult searchResult;

            var searchCriteria = GetSearchCriteria(request);

            do
            {
                searchCriteria.Take = take;
                searchCriteria.Skip = skip;

                searchResult = await _pushMessageSearchService.SearchAsync(searchCriteria);

                pushMessages.AddRange(searchResult.Results);
                skip += take;
            }
            while (searchResult.Results.Count == take);

            var messagesCombined = await _pushMessageService.GetRecipientsMessages(pushMessages, request.UnreadOnly ? false : null);

            var result = new ExpPushMessagesResponse();

            foreach (var messageCombined in messagesCombined)
            {
                foreach (var recipient in messageCombined.Recipients)
                {
                    var expPushMessage = new ExpPushMessage
                    {
                        Id = messageCombined.Message.Id,
                        ShortMessage = messageCombined.Message.ShortMessage,
                        CreatedDate = messageCombined.Message.CreatedDate,
                        UserId = recipient.UserId,
                    };

                    result.Items.Add(expPushMessage);
                }
            }

            result.UnreadCount = result.Items.Count(x => x.Status == "Unread");

            return result;
        }

        private static PushMessageSearchCriteria GetSearchCriteria(GetPushMessagesQuery request)
        {
            var criteria = AbstractTypeFactory<PushMessageSearchCriteria>.TryCreateInstance();
            criteria.UserId = request.UserId;
            criteria.IsRead = request.UnreadOnly ? false : null;
            return criteria;
        }
    }
}
