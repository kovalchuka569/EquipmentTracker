using System.Dynamic;

namespace Models.Equipment;

public class EquipmentRow : DynamicObject
{
    public int Id { get; set; }
    public ExpandoObject Data { get; set; } = new();

    private IDictionary<string, object?> Dict => Data;
    
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        if (binder.Name == nameof(Id))
        {
            result = Id;
            return true;
        }
        if (binder.Name == nameof(Data))
        {
            result = Data;
            return true;
        }
        
        return Dict.TryGetValue(binder.Name, out result);
    }
    
    public override bool TrySetMember(SetMemberBinder binder, object? value)
    {
        if (binder.Name == nameof(Id) || binder.Name == nameof(Data))
        {
            return false;
        }

        Dict[binder.Name] = value;
        return true;
    }
    
    public override IEnumerable<string> GetDynamicMemberNames() => Dict.Keys;
}