using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class ConfigBase<T> : ScriptableObject where T: ConfigStructure
{
    protected abstract List<T> GetData();

    public T Get(Func<T, int> getIndex, int checkValue)
    {
        List<T> data = GetData();
        int min = 0, max = data.Count - 1, middle, middleValue;        
        while (min <= max)
        {
            middle = min + (max - min) / 2;
            middleValue = getIndex(data[middle]);
            if (middleValue == checkValue)
            {
                return data[middle];
            }
            else if (checkValue > middleValue)
            {
                min = middle + 1;
            }
            else
            {
                max = middle - 1;
            }
        }
        return null;
    }

    public T Get(Func<T, bool> check)
    {
        List<T> data = GetData();
        for (int i = 0; i < data.Count; i++)
        {
            if (check(data[i]))
            {
                return data[i];
            }
        }
        return null;
    }
}

public abstract class ConfigStructure
{
}

public abstract class Localizable: ConfigStructure
{
    public string vi;
    public string en;

    public string Value
    {
        get
        {
            //base on localization will return the correct value
            return vi;
        }
    }
}
