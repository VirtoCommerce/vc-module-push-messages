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
            result.Items = await SearchMessages(request);
            result.UnreadCount = result.Items.Count(x => !x.IsRead);

            return result;
        }

        private async Task<List<ExpPushMessage>> SearchMessages(GetPushMessagesQuery request)
        {
            var criteria = GetSearchCriteria(request);
            var messages = await _recipientSearchService.SearchAllNoCloneAsync(criteria);

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
            criteria.IsRead = request.UnreadOnly ? false : null;
            criteria.Take = 50;
            criteria.ResponseGroup = PushMessageRecipientResponseGroup.WithMessages.ToString();

            return criteria;
        }
    }
}
