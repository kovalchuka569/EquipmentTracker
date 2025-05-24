namespace Models.RepairsDataGrid.AddService;

public class ServiceData
{
    public int EquipmentId { get; set; }
    public DateTime? StartService { get; set; }
    public DateTime? EndService { get; set; }
    public TimeSpan? TimeSpentOnService { get; set; }
    public string? ServiceDescription { get; set; }
    public int Worker { get; set; }
    public string ServiceStatus { get; set; }
    public string ServiceType { get; set; }
}