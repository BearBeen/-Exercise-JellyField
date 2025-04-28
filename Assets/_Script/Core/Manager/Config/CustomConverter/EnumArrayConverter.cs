#if UNITY_EDITOR
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using System;

public class EnumArrayConverter<T> : DefaultTypeConverter where T: Enum
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string[] strEnums = text.Split(ConfigManager.ArraySeparator);
        int[] ints = new int[strEnums.Length];
        T[] result = new T[strEnums.Length];
        for (int i = 0; i < strEnums.Length; i++)
        {
            if (int.TryParse(strEnums[i], out ints[i]))
            {
                result[i] = (T)Enum.ToObject(typeof(T), ints[i]);
            }
            else
            {

                result[i] = (T)Enum.Parse(typeof(T), strEnums[i]);
            }
        }
        return result;
    }
}
#endif