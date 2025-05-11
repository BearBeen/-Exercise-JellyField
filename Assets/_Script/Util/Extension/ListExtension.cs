using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ListExtension
{
    public static List<T> GetRandoms<T>(this List<T> list, int amount)
    {
        if (amount == 0) return new List<T>();
        int count = list.Count;
        if (amount >= count) return list;

        List<T> result = new List<T>();
        List<T> reServed = new List<T>(list); //to protect the input list.
        while (amount > 0)
        {
            int index = Random.Range(0, count);
            result.Add(reServed[index]);
            reServed[index] = reServed[--count];
            --amount;
        }
        return result;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        if (list == null || list.Count == 0) return default;
        return list[Random.Range(0, list.Count)];
    }
}
