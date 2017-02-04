using System.Collections.Generic;

namespace HandyQuery.Language.Extensions
{
    internal static class ListExtensions
    {
        /// <summary>
        /// Checks if lists constains same elements. Order does matter. Uses Equals method.
        /// </summary>
        public static bool IsSameAs<T>(this List<T> items, List<T> items2)
        {
            for (var i = 0; i < items2.Count; i++)
            {
                if (items2[i].Equals(items[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}