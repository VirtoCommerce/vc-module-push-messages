using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Settings;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;
using GeneralSettings = VirtoCommerce.PushMessages.Core.ModuleConstants.Settings.General;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkAllPushMessagesUnreadCommandHandler : IRequestHandler<MarkAllPushMessagesUnreadCommand, bool>
    {
        private readonly IPushMessageRecipientService _recipientService;
        private readonly IPushMessageRecipientSearchService _recipientSearchService;
        private readonly ISettingsManager _settingsManager;

        public MarkAllPushMessagesUnreadCommandHandler(
            IPushMessageRecipientService recipientService,
            IPushMessageRecipientSearchService recipientSearchService,
            ISettingsManager settingsManager)
        {
            _recipientService = recipientService;
            _recipientSearchService = recipientSearchService;
            _settingsManager = settingsManager;
        }

        public async Task<bool> Handle(MarkAllPushMessagesUnreadCommand request, CancellationToken cancellationToken)
        {
            var searchCriteria = AbstractTypeFactory<PushMessageRecipientSearchCriteria>.TryCreateInstance();
            searchCriteria.UserId = request.UserId;
            searchCriteria.IsRead = true;
            searchCriteria.WithHidden = true;
            searchCriteria.Take = await _settingsManager.GetValueAsync<int>(GeneralSettings.BatchSize);

            await _recipientSearchService.SearchWhileResultIsNotEmpty(searchCriteria, async searchResult =>
            {
                searchResult.Results.Apply(x => x.IsRead = false);
                await _recipientService.SaveChangesAsync(searchResult.Results);
            });

            return true;
        }
    }
}
