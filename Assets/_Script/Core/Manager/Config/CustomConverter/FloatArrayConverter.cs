#if UNITY_EDITOR
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;

public class FloatArrayConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrEmpty(text))
        {
            return null;
        }

        string[] strFloats = text.Split(ConfigManager.ArraySeparator);
        float[] result = new float[strFloats.Length];
        for (int i = 0; i < strFloats.Length; i++)
        {
            result[i] = float.Parse(strFloats[i]);
        }
        return result;
    }
}
#endif