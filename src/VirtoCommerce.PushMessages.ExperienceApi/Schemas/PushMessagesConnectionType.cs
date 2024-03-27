using System.Collections.Generic;
using GraphQL.Types;
using GraphQL.Types.Relay;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessagesConnectionType<TNodeType> : ConnectionType<TNodeType, EdgeType<TNodeType>>
        where TNodeType : IGraphType
    {
        public PushMessagesConnectionType()
        {
            Field<NonNullGraphType<IntGraphType>>("unreadCount", description: "Unread messages count",
                resolve: context => ((PushMessagesConnection<ExpPushMessage>)context.Source).UnreadCount);
        }
    }

    public class PushMessagesConnection<TNode> : PagedConnection<TNode>
    {
        public PushMessagesConnection(IEnumerable<TNode> superset, int skip, int take, int totalCount)
            : base(superset, skip, take, totalCount)
        {
        }

        public int UnreadCount { get; set; }
    }
}
