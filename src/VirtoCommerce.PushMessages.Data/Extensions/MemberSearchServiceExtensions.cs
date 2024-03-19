using System.Collections.Generic;
using System.Linq;
using VirtoCommerce.CustomerModule.Core.Model.Search;
using VirtoCommerce.CustomerModule.Core.Services;
using VirtoCommerce.Platform.Core.Common;

namespace VirtoCommerce.PushMessages.Data.Extensions;

public static class MemberSearchServiceExtensions
{
    public static async IAsyncEnumerable<MemberSearchResult> SearchBatchesAsync(this IMemberSearchService searchService, MembersSearchCriteria searchCriteria)
    {
        int totalCount;
        searchCriteria = searchCriteria.CloneTyped();

        do
        {
            var searchResult = await searchService.SearchMembersAsync(searchCriteria);

            if (searchCriteria.Take == 0 ||
                searchResult.Results.Any())
            {
                yield return searchResult;
            }

            if (searchCriteria.Take == 0)
            {
                yield break;
            }

            totalCount = searchResult.TotalCount;
            searchCriteria.Skip += searchCriteria.Take;
        }
        while (searchCriteria.Skip < totalCount);
    }
}
