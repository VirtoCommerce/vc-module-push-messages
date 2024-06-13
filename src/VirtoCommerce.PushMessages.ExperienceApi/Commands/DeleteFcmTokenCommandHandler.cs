using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class DeleteFcmTokenCommandHandler : IRequestHandler<DeleteFcmTokenCommand, bool>
{
    private readonly IFcmTokenService _fcmTokenService;
    private readonly IFcmTokenSearchService _fcmTokenSearchService;

    public DeleteFcmTokenCommandHandler(
        IFcmTokenService fcmTokenService,
        IFcmTokenSearchService fcmTokenSearchService)
    {
        _fcmTokenService = fcmTokenService;
        _fcmTokenSearchService = fcmTokenSearchService;
    }

    public async Task<bool> Handle(DeleteFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var searchCriteria = AbstractTypeFactory<FcmTokenSearchCriteria>.TryCreateInstance();
        searchCriteria.Token = request.Token;
        searchCriteria.UserIds = [request.UserId];

        await _fcmTokenSearchService.SearchWhileResultIsNotEmpty(searchCriteria, async searchResult =>
        {
            var ids = searchResult.Results.Select(x => x.Id).ToList();
            await _fcmTokenService.DeleteAsync(ids);
        });

        return true;
    }
}
