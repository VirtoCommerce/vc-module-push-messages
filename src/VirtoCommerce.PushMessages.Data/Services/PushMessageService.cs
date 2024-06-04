using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.Platform.Caching;
using VirtoCommerce.Platform.Core.Caching;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Events;
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

    public PushMessageService(
        Func<IPushMessagesRepository> repositoryFactory,
        IPlatformMemoryCache platformMemoryCache,
        IEventPublisher eventPublisher)
        : base(repositoryFactory, platformMemoryCache, eventPublisher)
    {
        _repositoryFactory = repositoryFactory;
    }

    public virtual async Task<PushMessage> ChangeTracking(string messageId, bool value)
    {
        var message = await this.GetByIdAsync(messageId);

        if (message != null && message.TrackNewRecipients != value)
        {
            message.TrackNewRecipients = value;

            // Skip status validation
            await base.SaveChangesAsync([message]);
        }

        return message;
    }

    public override async Task<IList<PushMessage>> GetAsync(IList<string> ids, string responseGroup = null, bool clone = true)
    {
        var withReadRate = HasFlag(responseGroup, PushMessageResponseGroup.WithReadRate);
        var models = await base.GetAsync(ids, responseGroup, clone || withReadRate);

        if (withReadRate && models.Count > 0)
        {
            await CalculateReadRate(models);
        }

        return models;
    }

    public override async Task SaveChangesAsync(IList<PushMessage> models)
    {
        var ids = models.Where(x => x.Id != null).Select(x => x.Id).ToList();
        await ValidateStatusAsync(ids);
        await base.SaveChangesAsync(models);
    }

    public override async Task DeleteAsync(IList<string> ids, bool softDelete = false)
    {
        await ValidateStatusAsync(ids);
        await base.DeleteAsync(ids, softDelete);
    }


    protected override Task<IList<PushMessageEntity>> LoadEntities(IRepository repository, IList<string> ids, string responseGroup)
    {
        return ((IPushMessagesRepository)repository).GetMessagesByIdsAsync(ids, responseGroup);
    }

    protected override void ClearCache(IList<PushMessage> models)
    {
        base.ClearCache(models);

        GenericCachingRegion<PushMessageRecipient>.ExpireRegion();
        GenericSearchCachingRegion<PushMessageRecipient>.ExpireRegion();
    }


    private async Task ValidateStatusAsync(IList<string> ids)
    {
        var models = await GetAsync(ids);

        if (models.Any(x => x.Status == PushMessageStatus.Sent))
        {
            throw new InvalidOperationException($"Cannot modify or delete messages with status {PushMessageStatus.Sent}.");
        }
    }

    private static bool HasFlag(string responseGroup, PushMessageResponseGroup flag)
    {
        var responseGroupEnum = EnumUtility.SafeParseFlags(responseGroup, PushMessageResponseGroup.Default);
        return responseGroupEnum.HasFlag(flag);
    }

    private async Task CalculateReadRate(IList<PushMessage> messages)
    {
        using var repository = _repositoryFactory();
        var ids = messages.Select(x => x.Id).ToList();

        var recipients = await repository.Recipients
            .Where(x => ids.Contains(x.MessageId))
            .GroupBy(x => x.MessageId)
            .Select(g => new { MessageId = g.Key, TotalCount = g.Count(), ReadCount = g.Count(x => x.IsRead) })
            .ToListAsync();

        foreach (var recipient in recipients)
        {
            var message = messages.First(x => x.Id.EqualsIgnoreCase(recipient.MessageId));
            message.RecipientsTotalCount = recipient.TotalCount;
            message.RecipientsReadCount = recipient.ReadCount;
            message.RecipientsReadPercent = CalculatePercent(recipient.ReadCount, recipient.TotalCount);
        }
    }

    private static int CalculatePercent(int readCount, int totalCount)
    {
        if (readCount == 0 || totalCount == 0)
        {
            return 0;
        }

        var percent = (int)Math.Round((double)readCount / totalCount * 100, MidpointRounding.AwayFromZero);

        if (percent == 100 && readCount < totalCount)
        {
            percent = 99;
        }

        return percent;
    }
}
