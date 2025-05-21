namespace Models.EquipmentDataGrid;

public class EquipmentItem
{
    public int Id { get; set; }
    public string InventoryNumber { get; set; }
    public string Brand { get; set; }
    public string Model { get; set; }
    public string SerialNumber { get; set; }
    public string Class { get; set; }
    public string Year { get; set; }
    
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
    
    public string InvenoryNumberDisplay => InventoryNumber;
    public string BrandDisplay => Brand;
    public string ModelDisplay => Model;
    public string SerialNumberDisplay => SerialNumber;
    public string ClassDisplay => Class;
    public string YearDisplay => $"{Year} р";
    
    public string HeightDisplay => $"{Height} см";
    public string WidthDisplay => $"{Width} см";
    public string LengthDisplay => $"{Length} см";
    public string WeightDisplay => $"{Weight} кг";
    
    public string FloorDisplay => Floor;
    public string DepartmentDisplay => Department;
    public string RoomDisplay => Room;
    
    public string ConsumptionDisplay => $"{Consumption} (КВ/год)";
    public string VoltageDisplay => $"{Voltage} (В)";
    public string WaterDisplay => $"{Water} (Л/год)";
    public string AirDisplay => $"{Air} (Л/год)";
    
    public string BalanceCostDisplay => $"{BalanceCost} грн";
    public string NotesDisplay => Notes;  
    public string ResponsiblePersonDisplay => ResponsiblePerson;
}