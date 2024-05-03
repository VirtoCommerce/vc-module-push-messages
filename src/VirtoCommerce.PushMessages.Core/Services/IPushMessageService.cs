using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.GenericCrud;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Core.Services;

public interface IPushMessageService : ICrudService<PushMessage>
{
    Task<PushMessage> ChangeTracking(string messageId, bool value);
}
