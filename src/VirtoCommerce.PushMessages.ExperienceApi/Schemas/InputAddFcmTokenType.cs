using GraphQL.Types;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class InputAddFcmTokenType : InputObjectGraphType
    {
        public InputAddFcmTokenType()
        {
            Field<NonNullGraphType<StringGraphType>>("token");
        }
    }
}
