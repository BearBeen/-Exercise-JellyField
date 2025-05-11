using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ArrayExtension
{
    public static T[] GetRandoms<T>(this T[] array, int amount)
    {
        if (amount == 0) return new T[0];
        int count = array.Length;
        if (amount >= count) return array;

        T[] result = new T[amount];
        T[] reServed = new T[count]; //to protect the input array.
        Array.Copy(array, reServed, count);
        while (amount > 0)
        {
            int index = UnityEngine.Random.Range(0, count);
            result[--amount] = reServed[index];
            reServed[index] = reServed[--count];
        }
        return result;
    }

    public static T GetRandom<T>(this T[] array)
    {
        if (array == null || array.Length == 0) return default;
        return array[UnityEngine.Random.Range(0, array.Length)];
    }
}
