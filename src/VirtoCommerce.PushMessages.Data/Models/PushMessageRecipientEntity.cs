using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Models;

public class PushMessageRecipientEntity : AuditableEntity
{

    [StringLength(128)]
    public string MessageId { get; set; }

    [StringLength(128)]
    public string UserId { get; set; }

    public bool IsRead { get; set; }

    public PushMessageEntity Message { get; set; }

    public virtual PushMessageRecipient ToModel(PushMessageRecipient model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.MessageId = MessageId;
        model.UserId = UserId;
        model.IsRead = IsRead;

        return model;
    }
}
