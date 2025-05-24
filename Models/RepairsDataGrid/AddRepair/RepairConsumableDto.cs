namespace Models.RepairsDataGrid.AddRepair;

public class RepairConsumableDto
{
    public int MaterialId { get; set; }
    public string ConsumableTableName { get; set; }
    public string OperationsConsumableTableName => $"{ConsumableTableName} операції";
    public string Category { get; set; }
    public string Name { get; set; }
    public string Unit { get; set; }
    public decimal? SpentMaterial { get; set; }
}