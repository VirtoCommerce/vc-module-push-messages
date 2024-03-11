using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
using VirtoCommerce.Platform.Core.Security;
using VirtoCommerce.Platform.Data.GenericCrud;
using VirtoCommerce.PushMessages.Core.Events;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using VirtoCommerce.PushMessages.Data.Models;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageService : CrudService<PushMessage, PushMessageEntity, PushMessageChangingEvent, PushMessageChangedEvent>, IPushMessageService
{
    private readonly Func<IPushMessagesRepository> _repositoryFactory;
    private readonly IMemberService _memberService;

    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IMemberService memberService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _memberService = memberService;
    }

    public virtual async Task<PushMessageRecipient> UpdateRecipientAsync(PushMessageRecipient recipient)
    {
        ArgumentNullException.ThrowIfNull(recipient);

        using var repository = _repositoryFactory();

        var entity = await repository.Recipients
            .Where(x =>
                x.MessageId == recipient.MessageId &&
                x.UserId == recipient.UserId)
            .FirstOrDefaultAsync();

        if (entity != null && entity.IsRead != recipient.IsRead)
        {
            entity.IsRead = recipient.IsRead;
            await repository.UnitOfWork.CommitAsync();
        }

        return entity?.ToModel(AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance());
    }


    protected override Task<IList<PushMessageEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IPushMessagesRepository)repository).GetMessagesByIdsAsync(ids, responseGroup);
    }

    protected override async Task AfterSaveChangesAsync(IList<PushMessage> models, IList<GenericChangedEntry<PushMessage>> changedEntries)
    {
        await base.AfterSaveChangesAsync(models, changedEntries);
        await AddNewRecipients(changedEntries);
    }

    private async Task AddNewRecipients(IList<GenericChangedEntry<PushMessage>> changedEntries)
    {
        using var repository = _repositoryFactory();

        foreach (var changedEntry in changedEntries.Where(x => x.EntryState is EntryState.Added or EntryState.Modified))
        {
            await AddNewRecipients(changedEntry, repository);
        }

        await repository.UnitOfWork.CommitAsync();
    }

    private async Task AddNewRecipients(GenericChangedEntry<PushMessage> changedEntry, IPushMessagesRepository repository)
    {
        var newMemberIds = changedEntry.NewEntry.MemberIds;

        if (newMemberIds.Count == 0)
        {
            return;
        }

        var members = await _memberService.GetByIdsAsync(newMemberIds.ToArray(), MemberResponseGroup.WithSecurityAccounts.ToString());

        // TODO: Get organization members

        var userIds = members.
            OfType<IHasSecurityAccounts>()
            .SelectMany(x => x.SecurityAccounts.Select(y => y.Id))
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return;
        }

        var messageId = changedEntry.NewEntry.Id;

        var existingUserIds = await repository.Recipients
            .Where(x => x.MessageId == messageId)
            .Select(x => x.UserId)
            .Distinct()
            .ToListAsync();

        var newUserIds = userIds.Except(existingUserIds).ToList();

        if (newUserIds.Count == 0)
        {
            return;
        }

        foreach (var newUserId in newUserIds)
        {
            var entity = AbstractTypeFactory<PushMessageRecipientEntity>.TryCreateInstance();
            entity.MessageId = messageId;
            entity.UserId = newUserId;

            repository.Add(entity);
        }
    }
}
