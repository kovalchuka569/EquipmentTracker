namespace Models.RepairsDataGrid.AddRepair;

public class RepairConsumableDto
{
    public string ConsumableTableName { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public double? SpentMaterial { get; set; }
}