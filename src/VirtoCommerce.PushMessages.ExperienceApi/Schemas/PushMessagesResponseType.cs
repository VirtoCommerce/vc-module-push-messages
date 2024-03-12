using GraphQL.Types;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessagesResponseType : ObjectGraphType<ExpPushMessagesResponse>
    {
        public PushMessagesResponseType()
        {
            Field(x => x.UnreadCount, nullable: false);
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<PushMessageType>>>>("items", resolve: x => x.Source.Items);
        }
    }
}
