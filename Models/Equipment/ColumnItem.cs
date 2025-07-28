using Models.Equipment.ColumnSettings;

namespace Models.Equipment;

public class ColumnItem
{
    public Guid Id { get; set; }
    public Guid EquipmentSheetId { get; set; }
    public ColumnSettingsDisplayModel Settings { get; set; }
}