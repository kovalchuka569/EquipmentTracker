namespace Models.RepairsDataGrid.AddRepair;

public class EquipmentItem
{
    public int EquipmentId { get; set; }
    public string EquipmentInventoryNumber { get; set; }
    public string EquipmentInventoryNumberDisplay => EquipmentInventoryNumber;
    public string EquipmentBrand { get; set; }
    public string EquipmentBrandDisplay => EquipmentBrand;
    public string EquipmentModel { get; set; }
    public string EquipmentModelDisplay => EquipmentModel;
    
    public string SelectedDisplayName => $"{EquipmentInventoryNumber} | {EquipmentBrand} | {EquipmentModel}";
}