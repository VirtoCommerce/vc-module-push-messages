using GraphQL.Types;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class InputMarkPushMessageReadType : InputObjectGraphType
    {
        public InputMarkPushMessageReadType()
        {
            Field<NonNullGraphType<StringGraphType>>("messageId");
        }
    }
}
