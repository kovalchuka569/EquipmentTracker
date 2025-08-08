using System.Windows;
using Newtonsoft.Json;

namespace Core.Common.JsonConverters;

public class FontWeightJsonConverter : JsonConverter<FontWeight>
{
    public override void WriteJson(JsonWriter writer, FontWeight value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override FontWeight ReadJson(JsonReader reader, Type objectType, FontWeight existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var weightString = reader.Value?.ToString();
            return (FontWeight)new FontWeightConverter().ConvertFromString(weightString); 
        }
        return default(FontWeight);
    }
}