using System;
using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQueryBuilder : SearchQueryBuilder<GetPushMessagesQuery, ExpPushMessagesResponse, ExpPushMessage, PushMessageType>
    {
        protected override string Name => "pushMessages";

        public GetPushMessagesQueryBuilder(IAuthorizationService authorizationService)
            : base(authorizationService)
        {
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public GetPushMessagesQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : this(authorizationService)
        {
        }

        protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, GetPushMessagesQuery request)
        {
            await Authorize(context, null, new PushMessagesAuthorizationRequirement());

            context.CopyArgumentsToUserContext();

            request.UserId = context.GetCurrentUserId();

            await base.BeforeMediatorSend(context, request);
        }
    }
}
