#if UNITY_EDITOR
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class IntergerArrayConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string[] strInts = text.Split(ConfigManager.ArraySeparator);
        int[] result = new int[strInts.Length];
        for (int i = 0; i < strInts.Length; i++)
        {
            result[i] = int.Parse(strInts[i]);
        }
        return result;
    }
}
#endif
