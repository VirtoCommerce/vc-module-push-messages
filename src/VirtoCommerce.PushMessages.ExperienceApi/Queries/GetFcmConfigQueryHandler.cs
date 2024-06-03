using System.Threading;
using System.Threading.Tasks;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmConfigQueryHandler : IQueryHandler<GetFcmConfigQuery, FcmConfig>
{
    public Task<FcmConfig> Handle(GetFcmConfigQuery request, CancellationToken cancellationToken)
    {
        var config = new FcmConfig
        {
            ApiKey = string.Empty,
            AuthDomain = string.Empty,
            ProjectId = string.Empty,
            StorageBucket = string.Empty,
            MessagingSenderId = string.Empty,
            AppId = string.Empty,
        };

        return Task.FromResult(config);
    }
}
