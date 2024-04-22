using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.Data.Models;

public class PushMessageEntity : AuditableEntity, IDataEntity<PushMessageEntity, PushMessage>
{
    [StringLength(128)]
    public string Topic { get; set; }

    [StringLength(1024)]
    public string ShortMessage { get; set; }

    public DateTime? StartDate { get; set; }

    [StringLength(64)]
    public string Status { get; set; }

    public virtual ObservableCollection<PushMessageMemberEntity> Members { get; set; } = new NullCollection<PushMessageMemberEntity>();

    public virtual ObservableCollection<PushMessageRecipientEntity> Recipients { get; set; } = new NullCollection<PushMessageRecipientEntity>();

    public virtual PushMessage ToModel(PushMessage model)
    {
        model.Id = Id;
        model.CreatedBy = CreatedBy;
        model.CreatedDate = CreatedDate;
        model.ModifiedBy = ModifiedBy;
        model.ModifiedDate = ModifiedDate;

        model.Topic = Topic;
        model.ShortMessage = ShortMessage;
        model.StartDate = StartDate;
        model.Status = Status;
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

        Topic = model.Topic;
        ShortMessage = model.ShortMessage;
        StartDate = model.StartDate;
        Status = model.Status;

        if (model.MemberIds != null)
        {
            Members = [];
            foreach (var memberId in model.MemberIds)
            {
                Members.Add(CreateMemberEntity(model.Id, memberId));
            }
        }

        return this;
    }

    public virtual void Patch(PushMessageEntity target)
    {
        target.Topic = Topic;
        target.ShortMessage = ShortMessage;
        target.StartDate = StartDate;
        target.Status = Status;

        if (!Members.IsNullCollection())
        {
            var comparer = AnonymousComparer.Create((PushMessageMemberEntity x) => x.MemberId);
            Members.Patch(target.Members, comparer, (sourceEntity, targetEntity) => targetEntity.MemberId = sourceEntity.MemberId);
        }
    }


    private static PushMessageMemberEntity CreateMemberEntity(string messageId, string memberId)
    {
        var memberEntity = AbstractTypeFactory<PushMessageMemberEntity>.TryCreateInstance();
        memberEntity.MessageId = messageId;
        memberEntity.MemberId = memberId;
        return memberEntity;
    }
}
