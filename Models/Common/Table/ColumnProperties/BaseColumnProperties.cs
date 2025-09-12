using Models.Equipment;

namespace Models.Common.Table.ColumnProperties;

public class BaseColumnProperties
{
    public Guid Id { get; set; }
    
    public string MappingName { get; set; } = string.Empty;
    
    public string HeaderText { get; set; } = string.Empty;
    
    public long Order { get; set; }
    
    public ColumnDataType ColumnDataType { get; set; }
    
    public bool HasDefaultValue { get; set; }
    
    public object? DefaultValue { get; set; }
    
    public double HeaderWidth { get; set; }
    
    public bool IsFrozen { get; set; }
    
    public bool IsUnique { get; set; }
    
    public bool IsRequired { get; set; }
    
    public bool MarkedForDelete { get; set; }
}