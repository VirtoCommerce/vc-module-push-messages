using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Commands
{
    public class MarkPushMessageReadCommandBuilder : CommandBuilder<MarkPushMessageReadCommand, bool, InputMarkPushMessageReadType, BooleanGraphType>
    {
        protected override string Name => "markPushMessageRead";

        public MarkPushMessageReadCommandBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override async Task BeforeMediatorSend(IResolveFieldContext<object> context, MarkPushMessageReadCommand request)
        {
            await Authorize(context, null, new PushMessagesAuthorizationRequirement());

            request.UserId = context.GetCurrentUserId();

            await base.BeforeMediatorSend(context, request);
        }
    }
}
