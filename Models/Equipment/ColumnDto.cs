using Models.Equipment.ColumnSettings;

namespace Models.Equipment;

public class ColumnDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public ColumnSettingsDisplayModel Settings { get; set; }
}