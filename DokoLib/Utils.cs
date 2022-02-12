﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DokoLib;

/// <summary>
/// A general utility class.
/// </summary>
public static class Utils
{
    private static readonly Random rng = new();

    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        T[] result = new T[list.Count];

        for (int idx = 0; idx < list.Count; idx++)
        {
            int newIdx = rng.Next(list.Count);

            result[newIdx] = list[idx];
        }

        return result;
    }

    public static int IndexOf<T>(this IReadOnlyList<T> self, T elementToFind)
    {
        int i = 0;
        foreach (T element in self)
        {
            if (Equals(element, elementToFind))
                return i;
            i++;
        }
        return -1;
    }

    public static void ForEach<T>(this IEnumerable<T> self, Action<T> action)
    {
        foreach (T element in self)
        {
            action(element);
        }
    }

}