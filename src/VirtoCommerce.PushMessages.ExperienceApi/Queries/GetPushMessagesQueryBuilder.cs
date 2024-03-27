using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using GraphQL.Types.Relay;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.ExperienceApiModule.Core.Helpers;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Schemas;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQueryBuilder : QueryBuilder<GetPushMessagesQuery, ExpPushMessagesResponse, PushMessageType>
    {
        protected override string Name => "pushMessages";

        public GetPushMessagesQueryBuilder(IMediator mediator, IAuthorizationService authorizationService)
            : base(mediator, authorizationService)
        {
        }

        protected override FieldType GetFieldType()
        {
            var builder = GraphTypeExtenstionHelper.CreateConnection<PushMessageType, EdgeType<PushMessageType>, PushMessagesConnectionType<PushMessageType>, object>()
                .Name(Name)
                .PageSize(20);

            ConfigureArguments(builder.FieldType);

            builder.ResolveAsync(async context =>
            {
                var (query, response) = await Resolve(context);
                return new PushMessagesConnection<ExpPushMessage>(response.Items, query.Skip, query.Take, response.TotalCount)
                {
                    UnreadCount = response.UnreadCount,
                };
            });

            return builder.FieldType;
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
