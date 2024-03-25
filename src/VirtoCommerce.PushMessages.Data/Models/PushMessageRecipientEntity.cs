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
    public string MemberId { get; set; }

    [StringLength(512)]
    public string MemberName { get; set; }

    [StringLength(128)]
    public string UserId { get; set; }

    [StringLength(256)]
    public string UserName { get; set; }

    public bool IsRead { get; set; }

    public bool IsHidden { get; set; }

    public PushMessageEntity Message { get; set; }

    public virtual PushMessageRecipient ToModel(PushMessageRecipient model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.MessageId = MessageId;
        model.MemberId = MemberId;
        model.MemberName = MemberName;
        model.UserId = UserId;
        model.UserName = UserName;
        model.IsRead = IsRead;
        model.IsHidden = IsHidden;

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
        MemberId = model.MemberId;
        MemberName = model.MemberName;
        UserId = model.UserId;
        UserName = model.UserName;
        IsRead = model.IsRead;
        IsHidden = model.IsHidden;

        return this;
    }

    public virtual void Patch(PushMessageRecipientEntity target)
    {
        target.IsRead = IsRead;
        target.IsHidden = IsHidden;
    }
}
