using System.Collections.Generic;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageService : ICrudService<PushMessage>
{
    Task<IList<PushMessageCombined>> GetRecipientsMessages(IList<PushMessage> messageIds, bool? isRead);

    Task<PushMessageRecipient> UpdateRecipientAsync(PushMessageRecipient recipient);
}
