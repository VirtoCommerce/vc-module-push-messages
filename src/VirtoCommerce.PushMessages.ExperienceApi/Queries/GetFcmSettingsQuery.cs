using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.PushMessages.Core.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries;

public class GetFcmSettingsQuery : Query<FcmReceiverOptions>
{
    public string UserId { get; set; }

    public override IEnumerable<QueryArgument> GetArguments()
    {
        yield break;
    }

    public override void Map(IResolveFieldContext context)
    {
    }
}
