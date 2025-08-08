namespace Models.Common.Table.ColumnValidationRules;

public interface IColumnValidationRules
{
    public bool IsRequired { get; set; }
    public bool IsUnique { get; set; }
}