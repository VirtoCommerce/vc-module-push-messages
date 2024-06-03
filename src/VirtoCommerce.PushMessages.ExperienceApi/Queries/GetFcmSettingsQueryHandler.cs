using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmSettingsQueryHandler : IQueryHandler<GetFcmSettingsQuery, FcmSettings>
{
    public Task<FcmSettings> Handle(GetFcmSettingsQuery request, CancellationToken cancellationToken)
    {
        var config = new FcmSettings
        {
            ApiKey = string.Empty,
            AuthDomain = string.Empty,
            ProjectId = string.Empty,
            StorageBucket = string.Empty,
            MessagingSenderId = string.Empty,
            AppId = string.Empty,
            VapidKey = string.Empty,
        };

        return Task.FromResult(config);
    }
}
