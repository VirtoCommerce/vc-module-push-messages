using GraphQL.Types;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Schemas;

public class FcmSettingsType : ObjectGraphType<FcmReceiverOptions>
{
    public FcmSettingsType()
    {
        Field(x => x.ApiKey, nullable: false);
        Field(x => x.AuthDomain, nullable: false);
        Field(x => x.ProjectId, nullable: false);
        Field(x => x.StorageBucket, nullable: false);
        Field(x => x.MessagingSenderId, nullable: false);
        Field(x => x.AppId, nullable: false);
        Field(x => x.VapidKey, nullable: false);
    }
}
