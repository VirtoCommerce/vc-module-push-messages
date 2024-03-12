using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class ClearAllPushMessagesCommandHandler : IRequestHandler<ClearAllPushMessageCommand, bool>
    {
        private readonly IPushMessageService _pushMessageService;
        private readonly IPushMessageSearchService _pushMessageSearchService;

        public ClearAllPushMessagesCommandHandler(IPushMessageService pushMessageService, IPushMessageSearchService pushMessageSearchService)
        {
            _pushMessageService = pushMessageService;
            _pushMessageSearchService = pushMessageSearchService;
        }

        public async Task<bool> Handle(ClearAllPushMessageCommand request, CancellationToken cancellationToken)
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

                await _pushMessageService.DeleteAsync(searchResult.Results.Select(x => x.Id).ToArray());

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
