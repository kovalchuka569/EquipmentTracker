namespace Models.RepairsDataGrid.ServicesDataGrid;

public class ServiceDto
{
    public int Id { get; set; }
    public int EquipmentId { get; set; }
    public string EquipmentInventoryNumber { get; set; }
    public string EquipmentBrand { get; set; }
    public string EquipmentModel { get; set; }
    public string ServiceDescription { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeSpan? Duration { get; set; }
    public int Worker { get; set; }
    public string Type { get; set; }
    public string Status {get; set; }
}
