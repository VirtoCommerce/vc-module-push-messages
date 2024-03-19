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
        private readonly IPushMessageRecipientService _recipientService;
        private readonly IPushMessageRecipientSearchService _recipientSearchService;

        public ClearAllPushMessagesCommandHandler(
            IPushMessageRecipientService recipientService,
            IPushMessageRecipientSearchService recipientSearchService)
        {
            _recipientService = recipientService;
            _recipientSearchService = recipientSearchService;
        }

        public async Task<bool> Handle(ClearAllPushMessageCommand request, CancellationToken cancellationToken)
        {
            var searchCriteria = GetSearchCriteria(request);
            PushMessageRecipientSearchResult searchResult;

            do
            {
                searchResult = await _recipientSearchService.SearchAsync(searchCriteria);

                if (searchResult.Results.Count > 0)
                {
                    foreach (var recipient in searchResult.Results)
                    {
                        recipient.IsHidden = true;
                    }

                    await _recipientService.SaveChangesAsync(searchResult.Results);
                }
            }
            while (searchResult.Results.Count == searchCriteria.Take &&
                   searchResult.Results.Count != searchResult.TotalCount);

            return true;
        }

        private static PushMessageRecipientSearchCriteria GetSearchCriteria(PushMessagesCommand request)
        {
            var criteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            criteria.UserId = request.UserId;
            criteria.WithHidden = false;
            criteria.Take = 50;

            return criteria;
        }
    }
}
