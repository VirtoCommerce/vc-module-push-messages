using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.PushMessages.Core.Models;
using VirtoCommerce.Xapi.Core.BaseQueries;

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
