namespace Models.Common.Table.ColumnSpecificSettings;

public class CurrencyColumnSpecificSettings : IColumnSpecificSettings
{
    public string CurrencySymbol { get; set; }
    public CurrencyPosition CurrencyPosition {get; set; }
}

public enum CurrencyPosition
{
    Before,
    After
}