using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;
using VirtoCommerce.Xapi.Core.Extensions;
using VirtoCommerce.Xapi.Core.Helpers;
using VirtoCommerce.Xapi.Core.Infrastructure;
using VirtoCommerce.Xapi.Core.Security.Authorization;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessagesSchema(IPushMessageHub eventBroker, IAuthorizationService authorizationService)
        : ISchemaBuilder
    {
        public void Build(ISchema schema)
        {
            var messageAddedEventStreamFieldType = new FieldType
            {
                Name = "pushMessageCreated",
                Type = GraphTypeExtensionHelper.GetActualType<NonNullGraphType<PushMessageType>>(),
                Resolver = new FuncFieldResolver<ExpPushMessage>(ResolveMessage),
                StreamResolver = new SourceStreamResolver<ExpPushMessage>(Subscribe)
            };
            schema.Subscription.AddField(messageAddedEventStreamFieldType);
        }

        private ExpPushMessage ResolveMessage(IResolveFieldContext context)
        {
            return context.Source as ExpPushMessage;
        }

        private async ValueTask<IObservable<ExpPushMessage>> Subscribe(IResolveFieldContext context)
        {
            var authorizationResult = await authorizationService.AuthorizeAsync(context.GetCurrentPrincipal(), null, new PushMessagesAuthorizationRequirement());
            if (!authorizationResult.Succeeded)
            {
                throw AuthorizationError.AnonymousAccessDenied();
            }

            var currentUserId = context.GetCurrentUserId();
            return await eventBroker.MessagesAsync(currentUserId);
        }
    }
}
