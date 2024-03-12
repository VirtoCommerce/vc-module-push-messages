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
    private readonly IEventPublisher _eventPublisher;

    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IMemberService memberService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
        _eventPublisher = eventPublisher;
        _memberService = memberService;
    }

    public async Task<IList<PushMessageCombined>> GetRecipientsMessages(IList<PushMessage> messages, bool? isRead)
    {
        ArgumentNullException.ThrowIfNull(messages);

        using var repository = _repositoryFactory();

        var messageIds = messages.Select(x => x.Id);
        var recipientsByMessagesQuery = repository.Recipients.Where(x => messageIds.Contains(x.MessageId));

        if (isRead.HasValue)
        {
            recipientsByMessagesQuery = recipientsByMessagesQuery.Where(x => x.IsRead == isRead.Value);
        }

        var recipientsByMessages = await recipientsByMessagesQuery.GroupBy(x => x.MessageId)
           .Select(x => new { MessageId = x.Key, Items = x.ToList() })
           .ToListAsync();

        var result = new List<PushMessageCombined>();

        foreach (var recipientsByMessage in recipientsByMessages)
        {
            var message = messages.FirstOrDefault(x => x.Id == recipientsByMessage.MessageId);
            if (message != null)
            {
                var messageCombined = AbstractTypeFactory<PushMessageCombined>.TryCreateInstance();
                messageCombined.Message = message;
                messageCombined.Recipients = recipientsByMessage.Items.Select(x => x.ToModel(AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance())).ToList();
                result.Add(messageCombined);
            }
        }

        return result;
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

        var messagesWithRecipients = new List<PushMessageCombined>();
        foreach (var changedEntry in changedEntries.Where(x => x.EntryState is EntryState.Added or EntryState.Modified))
        {
            var recipients = await AddNewRecipients(changedEntry, repository);

            var pushMessageCombined = AbstractTypeFactory<PushMessageCombined>.TryCreateInstance();
            pushMessageCombined.Message = changedEntry.NewEntry;
            pushMessageCombined.Recipients = recipients;
            messagesWithRecipients.Add(pushMessageCombined);
        }

        await repository.UnitOfWork.CommitAsync();

        var entries = new List<GenericChangedEntry<PushMessageCombined>>();
        foreach (var messageWithRecipients in messagesWithRecipients)
        {
            var entry = new GenericChangedEntry<PushMessageCombined>(messageWithRecipients, EntryState.Added);
            entries.Add(entry);
        }
        await _eventPublisher.Publish(new PushMessageSendingEvent(entries));
    }

    private async Task<List<PushMessageRecipient>> AddNewRecipients(GenericChangedEntry<PushMessage> changedEntry, IPushMessagesRepository repository)
    {
        var result = new List<PushMessageRecipient>();

        var newMemberIds = changedEntry.NewEntry.MemberIds;

        if (newMemberIds.Count == 0)
        {
            return result;
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
            return result;
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
            return result;
        }

        foreach (var newUserId in newUserIds)
        {
            var entity = AbstractTypeFactory<PushMessageRecipientEntity>.TryCreateInstance();
            entity.MessageId = messageId;
            entity.UserId = newUserId;

            repository.Add(entity);

            var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
            recipient.MessageId = messageId;
            recipient.UserId = newUserId;

            result.Add(recipient);
        }

        return result;
    }
}
