using System.Collections.Generic;

namespace VirtoCommerce.PushMessages.Core.Models;

public class PushMessageCombined
{
    public PushMessage Message { get; set; }

    public List<PushMessageRecipient> Recipients { get; set; } = [];
}
