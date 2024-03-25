using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkPushMessageUnreadCommandHandler : IRequestHandler<MarkPushMessageUnreadCommand, bool>
    {
        private readonly IPushMessageRecipientService _recipientService;
        private readonly IPushMessageRecipientSearchService _recipientSearchService;

        public MarkPushMessageUnreadCommandHandler(
            IPushMessageRecipientService recipientService,
            IPushMessageRecipientSearchService recipientSearchService)
        {
            _recipientService = recipientService;
            _recipientSearchService = recipientSearchService;
        }

        public async Task<bool> Handle(MarkPushMessageUnreadCommand request, CancellationToken cancellationToken)
        {
            var criteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            criteria.MessageId = request.MessageId;
            criteria.UserId = request.UserId;
            criteria.IsRead = true;
            criteria.Take = 1;

            var searchResult = await _recipientSearchService.SearchAsync(criteria);

            var recipient = searchResult.Results.FirstOrDefault();
            if (recipient != null)
            {
                recipient.IsRead = false;
                await _recipientService.SaveChangesAsync([recipient]);
            }

            return true;
        }
    }
}
