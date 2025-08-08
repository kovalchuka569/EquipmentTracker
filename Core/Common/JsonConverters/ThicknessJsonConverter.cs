using System.Windows;
using Newtonsoft.Json;

namespace Core.Common.JsonConverters;

public class ThicknessJsonConverter : JsonConverter<Thickness>
{
    public override void WriteJson(JsonWriter writer, Thickness value, JsonSerializer serializer)
    {
        if (value.Left == value.Top && value.Top == value.Right && value.Right == value.Bottom)
        {
            writer.WriteValue(value.Left.ToString());
        }
        else
        {
            writer.WriteValue($"{value.Left},{value.Top},{value.Right},{value.Bottom}");
        }
    }

    public override Thickness ReadJson(JsonReader reader, Type objectType, Thickness existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            var thicknessString = reader.Value?.ToString();
            if (string.IsNullOrEmpty(thicknessString)) return default(Thickness);

            var parts = thicknessString.Split(',');
            if (parts.Length == 1 && double.TryParse(parts[0], out double uniform))
            {
                return new Thickness(uniform);
            }
            else if (parts.Length == 4 &&
                     double.TryParse(parts[0], out double left) &&
                     double.TryParse(parts[1], out double top) &&
                     double.TryParse(parts[2], out double right) &&
                     double.TryParse(parts[3], out double bottom))
            {
                return new Thickness(left, top, right, bottom);
            }
        }
        return default(Thickness);
    }
}