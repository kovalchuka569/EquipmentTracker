using Models.Equipment;

namespace Models.Summary.DataGrid;

public class ColumnWithMerge
{
    public int ColumnId { get; set; }
    public string? MergedWith { get; set; }
    public string HeaderText { get; set; }
    public string MappingName { get; set; }
}