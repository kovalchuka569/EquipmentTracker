namespace Models.Common.Table.ColumnValidationRules;

public class TextColumnValidationRules : IColumnValidationRules
{
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    
    public int MinLength { get; set; }
    public int MaxLength { get; set; }
}