using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Models;

public class PushMessageRecipientEntity : AuditableEntity, IDataEntity<PushMessageRecipientEntity, PushMessageRecipient>
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

        model.Message = Message?.ToModel(AbstractTypeFactory<PushMessage>.TryCreateInstance());

        return model;
    }

    public virtual PushMessageRecipientEntity FromModel(PushMessageRecipient model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        MessageId = model.MessageId;
        UserId = model.UserId;
        IsRead = model.IsRead;

        return this;
    }

    public virtual void Patch(PushMessageRecipientEntity target)
    {
        target.IsRead = IsRead;
    }
}
