using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Models;

public class PushMessageEntity : AuditableEntity, IDataEntity<PushMessageEntity, PushMessage>
{
    [StringLength(1024)]
    public string ShortMessage { get; set; }

    public virtual ObservableCollection<PushMessageMemberEntity> Members { get; set; } = new NullCollection<PushMessageMemberEntity>();

    public virtual ObservableCollection<PushMessageRecipientEntity> Recipients { get; set; } = new NullCollection<PushMessageRecipientEntity>();

    public virtual PushMessage ToModel(PushMessage model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.ShortMessage = ShortMessage;
        model.MemberIds = Members.OrderBy(x => x.MemberId).Select(x => x.MemberId).ToList();

        return model;
    }

    public virtual PushMessageEntity FromModel(PushMessage model, PrimaryKeyResolvingMap pkMap)
    {
        pkMap.AddPair(model, this);

        Id = model.Id;
        CreatedBy = model.CreatedBy;
        CreatedDate = model.CreatedDate;
        ModifiedBy = model.ModifiedBy;
        ModifiedDate = model.ModifiedDate;

        ShortMessage = model.ShortMessage;

        if (model.MemberIds != null)
        {
            Members = [];
            foreach (var memberId in model.MemberIds)
            {
                var memberEntity = AbstractTypeFactory<PushMessageMemberEntity>.TryCreateInstance();
                memberEntity.MemberId = memberId;
                memberEntity.MessageId = model.Id;
                Members.Add(memberEntity);
            }
        }

        return this;
    }

    public virtual void Patch(PushMessageEntity target)
    {
        target.ShortMessage = ShortMessage;

        if (!Members.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((PushMessageMemberEntity x) => x.MemberId);
            Members.Patch(target.Members, comparer, (sourceEntity, targetEntity) => targetEntity.MemberId = sourceEntity.MemberId);
        }
    }
}
