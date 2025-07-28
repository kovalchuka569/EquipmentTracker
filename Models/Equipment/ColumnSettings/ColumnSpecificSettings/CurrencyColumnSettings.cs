using Models.Table;

namespace Models.Equipment.ColumnSpecificSettings;

public class CurrencyColumnSettings : ColumnSpecificSettingsBase
{
    public string CurrencySymbol { get; set; }
    public bool PositionBefore { get; set; }
    public bool PositionAfter { get; set; }
}