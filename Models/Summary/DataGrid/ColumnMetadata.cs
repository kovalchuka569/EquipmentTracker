using Models.Equipment;

namespace Models.Summary.DataGrid;

public class ColumnMetadata
{
    public string HeaderText { get; set; }
    public string MappingName { get; set; }
    public ColumnDataType ColumnType { get; set; }
}