using System.Collections.Generic;
using GraphQL;
using GraphQL.Types;
using VirtoCommerce.Xapi.Core.BaseQueries;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQuery : SearchQuery<ExpPushMessagesResponse>
    {
        public bool UnreadOnly { get; set; }

        public bool WithHidden { get; set; }

        public string CultureName { get; set; }

        public string UserId { get; set; }

        public override IEnumerable<QueryArgument> GetArguments()
        {
            foreach (var argument in base.GetArguments())
            {
                yield return argument;
            }

            yield return Argument<BooleanGraphType>(nameof(UnreadOnly));
            yield return Argument<BooleanGraphType>(nameof(WithHidden));
            yield return Argument<StringGraphType>(nameof(CultureName));
        }

        public override void Map(IResolveFieldContext context)
        {
            base.Map(context);

            UnreadOnly = context.GetArgument<bool>(nameof(UnreadOnly));
            WithHidden = context.GetArgument<bool>(nameof(WithHidden));
            CultureName = context.GetArgument<string>(nameof(CultureName));
        }
    }
}
