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

    public AddFcmTokenCommandHandler(IFcmTokenService fcmTokenService)
    {
        _fcmTokenService = fcmTokenService;
    }

    public async Task<bool> Handle(AddFcmTokenCommand request, CancellationToken cancellationToken)
    {
        var fcmToken = AbstractTypeFactory<FcmToken>.TryCreateInstance();
        fcmToken.UserId = request.UserId;
        fcmToken.Token = request.Token;

        await _fcmTokenService.SaveChangesAsync([fcmToken]);

        return true;
    }
}
