namespace Models.RepairsDataGrid.AddRepair;

public class RepairData
{
    public int EquipmentId { get; set; }
    public DateTime? StartRepair { get; set; }
    public DateTime? EndRepair { get; set; }
    public TimeSpan? TimeSpentOnRepair { get; set; }
    public string? BreakDescription { get; set; }
    public int Worker { get; set; }
    public string RepairStatus { get; set; }
}