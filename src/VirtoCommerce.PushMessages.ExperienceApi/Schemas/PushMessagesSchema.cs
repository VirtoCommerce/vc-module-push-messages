using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using VirtoCommerce.ExperienceApiModule.Core.Extensions;
using VirtoCommerce.ExperienceApiModule.Core.Helpers;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Authorization;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessagesSchema(IPushMessageHub eventBroker, IAuthorizationService authorizationService) : ISchemaBuilder
    {
        private readonly IPushMessageHub _eventBroker = eventBroker;
        readonly IAuthorizationService _authorizationService = authorizationService;

        public void Build(ISchema schema)
        {
            var messageAddedEventStreamFieldType = new EventStreamFieldType
            {
                Name = "pushMessageCreated",
                Type = GraphTypeExtenstionHelper.GetActualType<NonNullGraphType<PushMessageType>>(),
                Resolver = new FuncFieldResolver<ExpPushMessage>(ResolveMessage),
                AsyncSubscriber = new AsyncEventStreamResolver<ExpPushMessage>(Subscribe)
            };
            schema.Subscription.AddField(messageAddedEventStreamFieldType);
        }

        private ExpPushMessage ResolveMessage(IResolveFieldContext context)
        {
            return context.Source as ExpPushMessage;
        }

        private async Task<IObservable<ExpPushMessage>> Subscribe(IResolveEventStreamContext context)
        {
            var authorizationResult = await _authorizationService.AuthorizeAsync(context.GetCurrentPrincipal(), null, new PushMessagesAuthorizationRequirement());
            if (!authorizationResult.Succeeded)
            {
                throw AuthorizationError.AnonymousAccessDenied();
            }

            var currentUserId = context.GetCurrentUserId();
            return await _eventBroker.MessagesAsync(currentUserId);
        }
    }
}
