using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmSettingsQueryBuilder : QueryBuilder<GetFcmSettingsQuery, FcmReceiverOptions, FcmSettingsType>
{
    protected override string Name => "fcmSettings";

    public GetFcmSettingsQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : base(mediator, authorizationService)
    {
    }

    protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, GetFcmSettingsQuery request)
    {
        await Authorize(context, null, new PushMessagesAuthorizationRequirement());
        context.CopyArgumentsToUserContext();
        request.UserId = context.GetCurrentUserId();

        await base.BeforeMediatorSend(context, request);
    }
}
