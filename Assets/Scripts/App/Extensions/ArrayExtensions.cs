using System;
using System.Collections.Generic;
namespace Driver.Extensions
{
    public static class ArrayExtensions
    {
        private static readonly Random Random = new Random();

        public static T[] FindAll<T>(this T[] array, Predicate<T> match)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            List<T> list = new List<T>();
            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    list.Add(array[i]);
                }
            }

            return list.ToArray();
        }

        public static bool TryGet<T>(this T[] array, Predicate<T> match, out T result)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            for (int i = 0; i < array.Length; i++)
            {
                if (match(array[i]))
                {
                    result = array[i];
                    return true;
                }
            }
            result = default;
            return false;
        }
        public static bool TryGet<T>(this List<T> list, Predicate<T> match, out T result)
        {
            if (list == null)
            {
                throw new ArgumentNullException("list");
            }

            if (match == null)
            {
                throw new ArgumentNullException("match");
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i]))
                {
                    result = list[i];
                    return true;
                }
            }
            result = default;
            return false;
        }

        public static void Swap<T>(this T[] array, int index1, int index2)
        {
            if (index1 < 0 || index1 >= array.Length || index2 < 0 || index2 >= array.Length)
            {
                throw new ArgumentOutOfRangeException("Index is out of range");
            }

            (array[index1], array[index2]) = (array[index2], array[index1]);
        }

        public static void Shuffle<T>(this T[] array)
        {
            for (int j = array.Length - 1; j > 0; j--)
            {
                int k = Random.Next(j + 1);
                (array[j], array[k]) = (array[k], array[j]);
            }
        }
    }
}