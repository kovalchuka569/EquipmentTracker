namespace Models.EquipmentDataGrid;

public class SparePartDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string SparePartName { get; set; }
    public string SparePartCategory { get; set; }
    public string SparePartSerialNumber { get; set; }
    public decimal SparePartQuantity { get; set; }
    public string SparePartUnit { get; set; }
    public string SparePartNotes { get; set; }
}