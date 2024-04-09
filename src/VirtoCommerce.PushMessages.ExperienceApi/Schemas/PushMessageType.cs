using GraphQL.Types;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas
{
    public class PushMessageType : ObjectGraphType<ExpPushMessage>
    {
        public PushMessageType()
        {
            Field(x => x.Id, nullable: false);
            Field(x => x.ShortMessage, nullable: false);
            Field(x => x.CreatedDate, nullable: false);
            Field(x => x.IsRead, nullable: false);
            Field(x => x.IsHidden, nullable: false);
        }
    }
}
