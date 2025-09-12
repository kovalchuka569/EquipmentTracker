namespace Models.Common.Table.ColumnProperties;

public class NumberColumnProperties : BaseColumnProperties
{
    public double MinNumberValue { get; set; }
    
    public double MaxNumberValue { get; set; }
    
    public int NumberDecimalDigits { get; set; }
}