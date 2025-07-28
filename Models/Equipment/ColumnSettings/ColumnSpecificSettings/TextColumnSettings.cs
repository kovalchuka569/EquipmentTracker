using Models.Table;

namespace Models.Equipment.ColumnSpecificSettings;

public class TextColumnSettings : ColumnSpecificSettingsBase
{
    public long MaxLength { get; set; }
    public long MinLength { get; set; }
}