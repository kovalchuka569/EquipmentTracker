namespace Models.EquipmentDataGrid;

public class EquipmentDto
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string Category { get; set; }
    public string SerialNumber { get; set; }
    public string Class { get; set; }
    public int? Year { get; set; }
    
    public decimal Height { get; set; }
    public decimal Width { get; set; }
    public decimal Length { get; set; }
    public decimal Weight { get; set; }
    
    public string Floor { get; set; }
    public string Department { get; set; }
    public string Room { get; set; }
    
    public decimal Consumption { get; set; }
    public decimal Voltage { get; set; }
    public decimal Water { get; set; }
    public decimal Air { get; set; }
    
    public decimal BalanceCost  { get; set; }
    public string Notes { get; set; }
    public string ResponsiblePerson { get; set; }
}