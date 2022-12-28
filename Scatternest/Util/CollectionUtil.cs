using System;
using System.Collections.Generic;

namespace Scatternest.Util
{
    public static class CollectionUtil
    {
        public static IEnumerable<T> AllButOne<T>(this IList<T> items, Random rng)
        {
            if (items.Count == 0) yield break;

            int excluded = rng.Next(items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                if (i != excluded)
                {
                    yield return items[i];
                }
            }
        }
    }
}
