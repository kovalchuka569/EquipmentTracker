using System.Windows.Media;
using Newtonsoft.Json;

namespace Core.Common.JsonConverters;

public class FontFamilyJsonConverter : JsonConverter<FontFamily>
{
    public override void WriteJson(JsonWriter writer, FontFamily? value, JsonSerializer serializer)
    {
        writer.WriteValue(value?.Source);
    }

    public override FontFamily? ReadJson(JsonReader reader, Type objectType, FontFamily? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var fontName = reader.Value?.ToString();
            if (!string.IsNullOrEmpty(fontName))
            {
                return new FontFamily(fontName);
            }
        }
        return null;
    }
}