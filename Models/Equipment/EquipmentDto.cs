namespace Models.Equipment;

public class EquipmentDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int RowIndex { get; set; }
    public IDictionary<string, object>? Data { get; set; }
}