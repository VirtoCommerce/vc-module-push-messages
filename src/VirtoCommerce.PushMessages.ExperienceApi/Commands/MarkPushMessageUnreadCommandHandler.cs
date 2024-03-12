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
        private readonly IPushMessageService _pushMessageService;

        public MarkPushMessageUnreadCommandHandler(IPushMessageService pushMessageService)
        {
            _pushMessageService = pushMessageService;
        }

        public async Task<bool> Handle(MarkPushMessageUnreadCommand request, CancellationToken cancellationToken)
        {
            var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
            recipient.MessageId = request.MessageId;
            recipient.UserId = request.UserId;
            recipient.IsRead = false;

            await _pushMessageService.UpdateRecipientAsync(recipient);

            return true;
        }
    }
}
