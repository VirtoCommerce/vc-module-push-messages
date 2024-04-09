using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkPushMessageReadCommandHandler : IRequestHandler<MarkPushMessageReadCommand, bool>
    {
        private readonly IPushMessageRecipientService _recipientService;
        private readonly IPushMessageRecipientSearchService _recipientSearchService;

        public MarkPushMessageReadCommandHandler(
            IPushMessageRecipientService recipientService,
            IPushMessageRecipientSearchService recipientSearchService)
        {
            _recipientService = recipientService;
            _recipientSearchService = recipientSearchService;
        }

        public async Task<bool> Handle(MarkPushMessageReadCommand request, CancellationToken cancellationToken)
        {
            var criteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            criteria.MessageId = request.MessageId;
            criteria.UserId = request.UserId;
            criteria.IsRead = false;
            criteria.WithHidden = true;
            criteria.Take = 1;

            var searchResult = await _recipientSearchService.SearchAsync(criteria);

            var recipient = searchResult.Results.FirstOrDefault();
            if (recipient != null)
            {
                recipient.IsRead = true;
                await _recipientService.SaveChangesAsync([recipient]);
            }

            return true;
        }
    }
}
