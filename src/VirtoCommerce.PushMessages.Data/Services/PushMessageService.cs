using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VirtoCommerce.CustomerModule.Core.Model;
using VirtoCommerce.CustomerModule.Core.Model.Search;
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
using VirtoCommerce.PushMessages.Data.Extensions;
using VirtoCommerce.PushMessages.Data.Models;
using VirtoCommerce.PushMessages.Data.Repositories;

namespace VirtoCommerce.PushMessages.Data.Services;

public class PushMessageService : CrudService<PushMessage, PushMessageEntity, PushMessageChangingEvent, PushMessageChangedEvent>, IPushMessageService
{
    private readonly IMemberService _memberService;
    private readonly IMemberSearchService _memberSearchService;
    private readonly IPushMessageRecipientService _recipientService;

    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher,
        IMemberService memberService,
        IMemberSearchService memberSearchService,
        IPushMessageRecipientService recipientService)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _memberService = memberService;
        _memberSearchService = memberSearchService;
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
        foreach (var changedEntry in changedEntries.Where(x =>
                     x.EntryState == EntryState.Added &&
                     x.NewEntry.MemberIds.Count > 0))
        {
            var message = changedEntry.NewEntry;
            var recipients = await GetRecipients(message);

            if (recipients.Count > 0)
            {
                // TODO: Use batches when saving
                await _recipientService.SaveChangesAsync(recipients);

                message.UserIds = recipients.Select(x => x.UserId).ToList();
            }
        }
    }

    private async Task<IList<PushMessageRecipient>> GetRecipients(PushMessage message)
    {
        List<PushMessageRecipient> recipients = [];

        var searchCriteria = AbstractTypeFactory<MembersSearchCriteria>.TryCreateInstance();
        searchCriteria.ResponseGroup = MemberResponseGroup.WithSecurityAccounts.ToString();
        searchCriteria.Take = 50;

        var members = await _memberService.GetByIdsAsync(message.MemberIds.ToArray(), searchCriteria.ResponseGroup);
        var queue = new Queue<Member>(members);

        while (queue.TryDequeue(out var member))
        {
            if (member is IHasSecurityAccounts hasSecurityAccounts)
            {
                recipients.AddRange(hasSecurityAccounts.SecurityAccounts.Select(x => GetRecipient(message, member, x)));
            }
            else
            {
                searchCriteria.MemberId = member.Id;

                await foreach (var searchResult in _memberSearchService.SearchBatchesAsync(searchCriteria))
                {
                    searchResult.Results.Apply(queue.Enqueue);
                }
            }
        }

        return recipients;
    }

    private static PushMessageRecipient GetRecipient(PushMessage message, Member member, ApplicationUser user)
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
