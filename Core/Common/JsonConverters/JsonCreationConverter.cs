using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Core.Common.JsonConverters;

public abstract class JsonCreationConverter<T> : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(T).IsAssignableFrom(objectType);
    }
    
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jsonObject = JObject.Load(reader);
        
        T target = Create(objectType, jsonObject);
        
        serializer.Populate(jsonObject.CreateReader(), target);

        return target;
    }
    
    protected abstract T Create(Type objectType, JObject jsonObject);
    
    public override bool CanWrite => false;
    
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException("This converter is only for reading JSON. Writing is handled by default serialization or another converter.");
    }
}