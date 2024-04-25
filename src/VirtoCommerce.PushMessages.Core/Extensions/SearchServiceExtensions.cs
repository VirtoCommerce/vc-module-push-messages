using System;
using System.Threading.Tasks;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.GenericCrud;

namespace VirtoCommerce.PushMessages.Core.Extensions
{
    public static class SearchServiceExtensions
    {
        public static async Task SearchWhileResultIsNotEmpty<TCriteria, TResult, TModel>(this ISearchService<TCriteria, TResult, TModel> searchService, TCriteria searchCriteria, Func<TResult, Task> action)
            where TCriteria : SearchCriteriaBase
            where TResult : GenericSearchResult<TModel>
            where TModel : IEntity
        {
            TResult searchResult;

            do
            {
                searchResult = await searchService.SearchAsync(searchCriteria);

                if (searchResult.Results.Count > 0)
                {
                    await action(searchResult);
                }
            }
            while (searchCriteria.Take > 0 &&
                   searchResult.Results.Count == searchCriteria.Take &&
                   searchResult.Results.Count != searchResult.TotalCount);
        }
    }
}
