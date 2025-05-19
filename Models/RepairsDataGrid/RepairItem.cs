namespace Models.RepairsDataGrid;

public class RepairItem
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string RepairDescription { get; set; }
    public string RepairDescriptionDisplay => RepairDescription;
    public string EquipmentInventoryNumber { get; set; }
    public string EquipmentInventoryNumberDisplay => EquipmentInventoryNumber;
    public string EquipmentBrand { get; set; }
    public string EquipmentBrandDisplay => EquipmentBrand;
    public string EquipmentModel { get; set; }
    public string EquipmentModelDisplay => EquipmentModel;
    public DateTime StartDate { get; set; }
    public string StartDateDisplay => StartDate.ToString("dd.MM.yyyy");
    public DateTime EndDate { get; set; }
    public string EndDateDisplay => EndDate.ToString("dd.MM.yyyy");
    public int Worker { get; set; }
    public string WorkerDisplay => Worker.ToString();
    public string Status {get; set; }
    public string StatusDisplay => Status;
}