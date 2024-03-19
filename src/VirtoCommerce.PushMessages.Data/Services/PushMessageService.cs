using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Caching;
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
    private readonly IMemberService _memberService;
    private readonly IPushMessageRecipientService _recipientService;

    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IMemberService memberService,
        IPushMessageRecipientService recipientService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _memberService = memberService;
        _recipientService = recipientService;
    }

    protected override Task<IList<PushMessageEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IPushMessagesRepository)repository).GetMessagesByIdsAsync(ids, responseGroup);
    }

    protected override async Task AfterSaveChangesAsync(IList<PushMessage> models, IList<GenericChangedEntry<PushMessage>> changedEntries)
    {
        await base.AfterSaveChangesAsync(models, changedEntries);
        await AddRecipients(changedEntries);
    }

    protected override void ClearCache(IList<PushMessage> models)
    {
        base.ClearCache(models);

        GenericCachingRegion<PushMessageRecipient>.ExpireRegion();
        GenericSearchCachingRegion<PushMessageRecipient>.ExpireRegion();
    }

    private async Task AddRecipients(IList<GenericChangedEntry<PushMessage>> changedEntries)
    {
        foreach (var changedEntry in changedEntries.Where(x => x.EntryState == EntryState.Added))
        {
            var message = changedEntry.NewEntry;
            var recipients = await CreateRecipients(message);
            message.UserIds = recipients.Select(x => x.UserId).ToList();
        }
    }

    private async Task<List<PushMessageRecipient>> CreateRecipients(PushMessage message)
    {
        if (message.MemberIds.Count == 0)
        {
            return [];
        }

        var members = await _memberService.GetByIdsAsync(message.MemberIds.ToArray(), MemberResponseGroup.WithSecurityAccounts.ToString());

        // TODO: Get organization members
        var recipients = members
            .SelectMany(x => BuildRecipients(message, x))
            .ToList();

        if (recipients.Count > 0)
        {
            await _recipientService.SaveChangesAsync(recipients);
        }

        return recipients;
    }

    private static IEnumerable<PushMessageRecipient> BuildRecipients(PushMessage message, Member member)
    {
        return member is IHasSecurityAccounts hasSecurityAccounts
            ? hasSecurityAccounts.SecurityAccounts.Select(x => BuildRecipient(message, member, x))
            : [];
    }

    private static PushMessageRecipient BuildRecipient(PushMessage message, Member member, ApplicationUser user)
    {
        var recipient = AbstractTypeFactory<PushMessageRecipient>.TryCreateInstance();
        recipient.MessageId = message.Id;
        recipient.MemberId = member.Id;
        recipient.MemberName = member.Name;
        recipient.UserId = user.Id;
        recipient.UserName = user.UserName;

        return recipient;
    }
}
