using System.Threading.Tasks;
using GraphQL;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQueryBuilder : QueryBuilder<GetPushMessagesQuery, ExpPushMessagesResponse, PushMessagesResponseType>
    {
        protected override string Name => "pushMessages";

        public GetPushMessagesQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override Task BeforeMediatorSend(IResolveFieldContext<object> context, GetPushMessagesQuery request)
        {
            context.CopyArgumentsToUserContext();
            request.UserId = context.GetCurrentUserId();

            return base.BeforeMediatorSend(context, request);
        }
    }
}
