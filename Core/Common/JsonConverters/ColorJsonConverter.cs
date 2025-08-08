using System.Windows.Media;
using Newtonsoft.Json;

namespace Core.Common.JsonConverters;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
            return (Color)ColorConverter.ConvertFromString(reader.Value?.ToString());
        
        throw new JsonSerializationException("Expected string for Color conversion");
    }
}