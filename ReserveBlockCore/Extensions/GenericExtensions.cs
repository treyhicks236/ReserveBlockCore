﻿namespace ReserveBlockCore.Extensions
{
    public static class GenericExtensions
    {
        public static IEnumerable<T> ToCircular<T>(this IEnumerable<T> source)
        {
            while (true)
            {
                foreach (var x in source) yield return x;
            }
        }
    }
}
