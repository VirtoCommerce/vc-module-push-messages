using System.Collections.Generic;
using System.Linq;

namespace VirtoCommerce.PushMessages.Core.Extensions;

public static class EnumerableExtensions
{
    public static IList<T> ToIList<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.ToList();
    }
}
