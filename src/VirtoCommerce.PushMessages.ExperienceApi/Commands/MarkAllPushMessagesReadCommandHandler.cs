using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkAllPushMessagesReadCommandHandler : IRequestHandler<MarkAllPushMessagesReadCommand, bool>
    {
        private readonly IPushMessageService _pushMessageService;
        private readonly IPushMessageSearchService _pushMessageSearchService;

        public MarkAllPushMessagesReadCommandHandler(IPushMessageService pushMessageService, IPushMessageSearchService pushMessageSearchService)
        {
            _pushMessageService = pushMessageService;
            _pushMessageSearchService = pushMessageSearchService;
        }

        public async Task<bool> Handle(MarkAllPushMessagesReadCommand request, CancellationToken cancellationToken)
        {
            var skip = 0;
            var take = 50;
            PushMessageSearchResult searchResult;

            var searchCriteria = GetSearchCriteria(request);

            do
            {
                searchCriteria.Take = take;
                searchCriteria.Skip = skip;

                searchResult = await _pushMessageSearchService.SearchAsync(searchCriteria);

                foreach (var pushMessage in searchResult.Results)
                {
                    var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
                    recipient.MessageId = pushMessage.Id;
                    recipient.UserId = request.UserId;
                    recipient.IsRead = true;

                    await _pushMessageService.UpdateRecipientAsync(recipient);
                }

                skip += take;
            }
            while (searchResult.Results.Count == take);

            return true;
        }

        private static PushMessageSearchCriteria GetSearchCriteria(PushMessagesCommand request)
        {
            var criteria = AbstractTypeFactory<PushMessageSearchCriteria>.TryCreateInstance();
            criteria.UserId = request.UserId;
            return criteria;
        }
    }
}
