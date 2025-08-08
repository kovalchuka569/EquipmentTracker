using Newtonsoft.Json;

namespace Models.Common.Table;

public class CellModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    // Ignored by the serializer since the cells in the database are stored as an array in a row
    [JsonIgnore]
    public Guid RowId { get; set; }
    
    public string ColumnMappingName { get; set; } = "";
    
    public object? Value { get; set; }
    
    public bool Deleted { get; set; }
}