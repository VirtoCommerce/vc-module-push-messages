using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.Core.Services;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands;

public class AddFcmTokenCommandHandler : IRequestHandler<AddFcmTokenCommand, bool>
{
    private readonly IFcmTokenService _fcmTokenService;
    private readonly IFcmTokenSearchService _fcmTokenSearchService;

    public AddFcmTokenCommandHandler(
        IFcmTokenService fcmTokenService,
        IFcmTokenSearchService fcmTokenSearchService)
    {
        _fcmTokenService = fcmTokenService;
        _fcmTokenSearchService = fcmTokenSearchService;
    }

    public async Task<bool> Handle(AddFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var searchCriteria = AbstractTypeFactory<FcmTokenSearchCriteria>.TryCreateInstance();
        searchCriteria.Token = request.Token;
        searchCriteria.UserIds = [request.UserId];

        var searchResult = await _fcmTokenSearchService.SearchAsync(searchCriteria);
        var fcmToken = searchResult.Results.FirstOrDefault();

        if (fcmToken is null)
        {
            fcmToken = AbstractTypeFactory<FcmToken>.TryCreateInstance();
            fcmToken.Token = request.Token;
            fcmToken.UserId = request.UserId;
        }

        await _fcmTokenService.SaveChangesAsync([fcmToken]);

        return true;
    }
}
