using Models.Common.Table.ColumnProperties;
using Models.Equipment;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Common.JsonConverters;

public class ColumnPropertiesJsonConverter : JsonConverter<BaseColumnProperties>
{
    public override void WriteJson(JsonWriter writer, BaseColumnProperties? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }
        
        var jo = new JObject();
        foreach (var prop in value.GetType().GetProperties())
        {
            var propValue = prop.GetValue(value);
            if (propValue != null)
            {
                if (prop.Name == nameof(BaseColumnProperties.ColumnDataType))
                    jo[prop.Name] = value.ColumnDataType.ToString();
                else
                    jo[prop.Name] = JToken.FromObject(propValue, serializer);
            }
        }

        jo.WriteTo(writer);
    }

    public override BaseColumnProperties? ReadJson(JsonReader reader, Type objectType, BaseColumnProperties? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return null;

        var jo = JObject.Load(reader);

        if (!jo.TryGetValue("ColumnDataType", out var typeToken))
            throw new JsonSerializationException("Missing ColumnDataType discriminator");

        var type = Enum.Parse<ColumnDataType>(typeToken.Value<string>()!);

        BaseColumnProperties instance = type switch
        {
            ColumnDataType.Text      => new TextColumnProperties(),
            ColumnDataType.Number    => new NumberColumnProperties(),
            ColumnDataType.Date      => new DateColumnProperties(),
            ColumnDataType.Currency  => new CurrencyColumnProperties(),
            ColumnDataType.List      => new ListColumnProperties(),
            ColumnDataType.Hyperlink => new LinkColumnProperties(),
            _ => throw new NotSupportedException($"Unsupported ColumnDataType: {type}")
        };

        serializer.Populate(jo.CreateReader(), instance);
        return instance;
    }
}