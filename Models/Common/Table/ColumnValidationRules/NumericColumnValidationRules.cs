namespace Models.Common.Table.ColumnValidationRules;

public class NumericColumnValidationRules : IColumnValidationRules
{
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
    
    public double? MinValue { get; set; }
    public double? MaxValue { get; set; }
}