using GraphQL.Types;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class InputMarkPushMessageUnreadType : InputObjectGraphType
    {
        public InputMarkPushMessageUnreadType()
        {
            Field<NonNullGraphType<StringGraphType>>("messageId");
        }
    }
}
