namespace Models.Common.Table.ColumnValidationRules;

public class DefaultColumnValidationRules : IColumnValidationRules
{
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
}