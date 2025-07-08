namespace Models.Equipment;

public class ColumnDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public ColumnSettings Settings { get; set; }
}