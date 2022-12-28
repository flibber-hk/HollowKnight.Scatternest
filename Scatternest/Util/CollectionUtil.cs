using System;
using System.Collections.Generic;
using System.Linq;

namespace Scatternest.Util
{
    public static class CollectionUtil
    {
        public static IEnumerable<T> AllButOne<T>(this IList<T> items, Random rng)
        {
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
