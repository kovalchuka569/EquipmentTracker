using Models.Equipment;
using Models.Equipment.ColumnSettings;

namespace Models.Summary.DataGrid;

public class ColumnItem
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int GroupId { get; set; }
    public ColumnSettingsDisplayModel ColumnSettings { get; set; }
}