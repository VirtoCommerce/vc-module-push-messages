using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageService : ICrudService<PushMessage>
{
    Task<PushMessageRecipient> UpdateRecipientAsync(PushMessageRecipient recipient);
}
