using Models.Equipment;

namespace Models.Summary.DataGrid;

public class ColumnItem
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public string TableName { get; set; }
    public ColumnSettings ColumnSettings { get; set; }
}