using GraphQL.Types;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class InputDeleteFcmTokenType : InputObjectGraphType
    {
        public InputDeleteFcmTokenType()
        {
            Field<NonNullGraphType<StringGraphType>>("token");
        }
    }
}
