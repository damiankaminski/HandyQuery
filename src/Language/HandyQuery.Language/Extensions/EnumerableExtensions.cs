using System.Collections.Generic;

namespace HandyQuery.Language.Extensions
{
    internal static class EnumerableExtensions
    {
        public static IEnumerable<T> WithoutDuplicates<T>(this IEnumerable<T> items)
        {
            var processed = new HashSet<T>();

            foreach (var item in items)
            {
                if(processed.Contains(item)) continue;
                processed.Add(item);
                yield return item;
            }
        }
    }
}