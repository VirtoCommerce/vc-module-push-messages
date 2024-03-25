using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkAllPushMessagesReadCommandBuilder : CommandBuilder<MarkAllPushMessagesReadCommand, bool, BooleanGraphType>
    {
        protected override string Name => "markAllPushMessagesRead";

        public MarkAllPushMessagesReadCommandBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, MarkAllPushMessagesReadCommand request)
        {
            await Authorize(context, null, new PushMessagesAuthorizationRequirement());

            request.UserId = context.GetCurrentUserId();

            await base.BeforeMediatorSend(context, request);
        }
    }
}
