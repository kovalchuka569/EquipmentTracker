namespace Models.Equipment;

public class ValidationSettings
{
    public ColumnDataType ColumnDataType { get; set; }
    public string MappingName { get; set; }
    public string HeaderText { get; set; }
    
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    
    // String
    public long? MaxTextLength { get; set; }
    public long? MinTextLength { get; set; }
    
    // Number
    public double? MinNumberValue { get; set; }
    public double? MaxNumberValue { get; set; }
}