using Models.Equipment;
using Models.Equipment.ColumnSettings;

namespace Models.Summary.DataGrid;

public class ReportGridColumn
{
    public string HeaderText { get; set; }
    public string MappingName { get; set; }
    public ColumnSettingsDisplayModel  ColumnSettings { get; set; }
}