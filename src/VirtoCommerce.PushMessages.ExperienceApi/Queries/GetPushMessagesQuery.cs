using System.Collections.Generic;
using GraphQL;
using GraphQL.Builders;
using GraphQL.Types;
using VirtoCommerce.ExperienceApiModule.Core.BaseQueries;
using VirtoCommerce.PushMessages.ExperienceApi.Models;

namespace VirtoCommerce.PushMessages.ExperienceApi.Queries
{
    public class GetPushMessagesQuery : Query<ExpPushMessagesResponse>
    {
        private const int PageSize = 20;

        public bool UnreadOnly { get; set; }

        public bool WithHidden { get; set; }

        public string CultureName { get; set; }

        public string UserId { get; set; }

        public string Keyword { get; set; }

        public int Skip { get; set; }

        public int Take { get; set; }

        public override IEnumerable<QueryArgument> GetArguments()
        {
            yield return Argument<BooleanGraphType>(nameof(UnreadOnly));
            yield return Argument<BooleanGraphType>(nameof(WithHidden));
            yield return Argument<StringGraphType>(nameof(CultureName));
            yield return Argument<StringGraphType>(nameof(Keyword));
        }

        public override void Map(IResolveFieldContext context)
        {
            UnreadOnly = context.GetArgument<bool>(nameof(UnreadOnly));
            WithHidden = context.GetArgument<bool>(nameof(WithHidden));
            CultureName = context.GetArgument<string>(nameof(CultureName));
            Keyword = context.GetArgument<string>(nameof(Keyword));

            if (context is IResolveConnectionContext connectionContext)
            {
                Skip = int.TryParse(connectionContext.After, out var skip) ? skip : 0;
                Take = connectionContext.First ?? connectionContext.PageSize ?? PageSize;
            }
        }
    }
}
