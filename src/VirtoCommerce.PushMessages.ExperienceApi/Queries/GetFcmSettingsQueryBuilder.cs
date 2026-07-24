using System;
using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmSettingsQueryBuilder : QueryBuilder<GetFcmSettingsQuery, FcmReceiverOptions, FcmSettingsType>
{
    protected override string Name => "fcmSettings";

    public GetFcmSettingsQueryBuilder(IAuthorizationService authorizationService)
        : base(authorizationService)
    {
    }

    [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
    public GetFcmSettingsQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
        : this(authorizationService)
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
