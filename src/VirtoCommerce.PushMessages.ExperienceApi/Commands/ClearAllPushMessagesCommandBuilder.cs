using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class ClearAllPushMessagesCommandBuilder : CommandBuilder<ClearAllPushMessageCommand, bool, BooleanGraphType>
    {
        protected override string Name => "clearAllPushMessages";

        public ClearAllPushMessagesCommandBuilder(IAuthorizationService authorizationService)
            : base(authorizationService)
        {
        }

        [Obsolete("Use the constructor without IMediator. The mediator is resolved from context.RequestServices per request.", DiagnosticId = "VC0015", UrlFormat = "https://docs.virtocommerce.org/products/products-virto3-versions")]
        public ClearAllPushMessagesCommandBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : this(authorizationService)
        {
        }

        protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, ClearAllPushMessageCommand request)
        {
            await Authorize(context, null, new PushMessagesAuthorizationRequirement());

            request.UserId = context.GetCurrentUserId();

            await base.BeforeMediatorSend(context, request);
        }
    }
}
