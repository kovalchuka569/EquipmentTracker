using Models.Table;

namespace Models.Equipment.ColumnSpecificSettings;

public class NumberColumnSettings : ColumnSpecificSettingsBase
{
    public int CharactersAfterComma { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
}