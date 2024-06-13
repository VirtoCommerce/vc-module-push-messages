using System.ComponentModel.DataAnnotations;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Models;

public class FcmTokenEntity : AuditableEntity, IDataEntity<FcmTokenEntity, FcmToken>
{
    [StringLength(256)]
    public string Token { get; set; }

    [StringLength(128)]
    public string UserId { get; set; }

    public virtual FcmToken ToModel(FcmToken model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Token = Token;
        model.UserId = UserId;

        return model;
    }

    public virtual FcmTokenEntity FromModel(FcmToken model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        Token = model.Token;
        UserId = model.UserId;

        return this;
    }

    public virtual void Patch(FcmTokenEntity target)
    {
        target.Token = Token;
        target.UserId = UserId;
    }
}
