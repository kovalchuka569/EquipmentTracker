namespace Models.Table;

public class ColumnSpecificSettingsNumber : ColumnSpecificSettingsBase
{
    public int NumberDecimalDigits { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
}