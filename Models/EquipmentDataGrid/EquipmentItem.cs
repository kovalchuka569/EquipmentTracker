namespace Models.EquipmentDataGrid;

public class EquipmentItem
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
    
    public string InventoryNumberDisplay => InventoryNumber;
    public string BrandDisplay => Brand;
    public string ModelDisplay => Model;
    public string CategoryDisplay => Category;
    public string SerialNumberDisplay => SerialNumber;
    public string ClassDisplay => Class;
    public string YearDisplay => Year.HasValue && Year.Value != 0 ? $"{Year} р" : string.Empty;
    public string HeightDisplay => Height > 0 ? $"{Height} см" : string.Empty;
    public string WidthDisplay => Width > 0 ? $"{Width} см" : string.Empty;
    public string LengthDisplay => Length > 0 ? $"{Length} см" : string.Empty;
    public string WeightDisplay => Weight > 0 ? $"{Weight} кг" : string.Empty;
    
    public string FloorDisplay => Floor;
    public string DepartmentDisplay => Department;
    public string RoomDisplay => Room;
    
    public string ConsumptionDisplay => Consumption > 0 ? $"{Consumption} (КВ/год)" : string.Empty;
    public string VoltageDisplay => Voltage > 0 ? $"{Voltage} (В)" : string.Empty;
    public string WaterDisplay => Water > 0 ? $"{Water} (Л/год)" : string.Empty;
    public string AirDisplay => Air > 0 ? $"{Air} (Л/год)" : string.Empty;
    
    public string BalanceCostDisplay => BalanceCost > 0 ? $"{BalanceCost} грн" : string.Empty;
    public string NotesDisplay => Notes;  
    public string ResponsiblePersonDisplay => ResponsiblePerson;
}