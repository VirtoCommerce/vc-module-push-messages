using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmSettingsQueryHandler : IQueryHandler<GetFcmSettingsQuery, FcmReceiverOptions>
{
    private readonly PushMessageOptions _options;

    public GetFcmSettingsQueryHandler(IOptions<PushMessageOptions> options)
    {
        _options = options.Value;
    }

    public Task<FcmReceiverOptions> Handle(GetFcmSettingsQuery request, CancellationToken cancellationToken)
    {
        var result = _options.UseFirebaseCloudMessaging
            ? _options.FcmReceiverOptions
            : null;

        return Task.FromResult(result);
    }
}
