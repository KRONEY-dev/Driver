using System;
using System.Collections.Generic;

namespace Driver.Extensions
{
    public static class ListExtensions
    {
        public static bool TryFind<T>(this List<T> list, Predicate<T> match, out T result)
        {
            result = default;
            if (list == null)
                return false;

            result = list.Find(match);
            return result != null;
        }

        public static bool RemoveFirst<T>(this List<T> list, Predicate<T> match)
        {
            
            if (list == null)
                return false;

            int index  = list.FindIndex(match);
            if (index == -1)
                return false;
            
            list.RemoveAt(index);
            return true;
        }
    }
}