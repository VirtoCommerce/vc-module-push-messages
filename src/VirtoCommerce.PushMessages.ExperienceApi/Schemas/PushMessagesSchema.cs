using System;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.Helpers;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PushMessages.ExperienceApi.Models;
using VirtoCommerce.PushMessages.ExperienceApi.Subscriptions;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessagesSchema(IPushMessageHub eventBroker) : ISchemaBuilder
    {
        private readonly IPushMessageHub _eventBroker = eventBroker;

        public void Build(ISchema schema)
        {
            var messageAddedEventStreamFieldType = new EventStreamFieldType
            {
                Name = "pushMessageCreated",
                Type = GraphTypeExtenstionHelper.GetActualType<PushMessageType>(),
                Resolver = new FuncFieldResolver<ExpPushMessage>(ResolveMessage),
                AsyncSubscriber = new AsyncEventStreamResolver<ExpPushMessage>(Subscribe)
            };
            schema.Subscription.AddField(messageAddedEventStreamFieldType);

            //var messageAddedToUserEventStreamFieldType = new EventStreamFieldType
            //{
            //    Name = "pushNotificationCreatedForUser",
            //    Type = typeof(PushMessageType),
            //    Resolver = new FuncFieldResolver<PushMessage>(ResolveMessage),
            //    AsyncSubscriber = new AsyncEventStreamResolver<PushMessage>(SubscribeToUser)
            //};
            //schema.Subscription.AddField(messageAddedToUserEventStreamFieldType);
        }

        private ExpPushMessage ResolveMessage(IResolveFieldContext context)
        {
            return context.Source as ExpPushMessage;
        }

        private Task<IObservable<ExpPushMessage>> Subscribe(IResolveEventStreamContext context)
        {
            return _eventBroker.MessagesAsync();
        }

        //private Task<IObservable<PushMessage>> SubscribeToUser(IResolveEventStreamContext context)
        //{
        //    var currentUserId = context.GetCurrentUserId();

        //    var result = _eventBroker.MessagesByUserIdAsync(currentUserId);

        //    return result;
        //}
    }
}
