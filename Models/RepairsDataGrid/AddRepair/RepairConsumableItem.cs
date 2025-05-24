namespace Models.RepairsDataGrid.AddRepair;

public class RepairConsumableItem
{
    public string ConsumableTableName { get; set; }
    public int MaterialId { get; set; }
    public string Category { get; set; }
    public string CategoryDisplay => Category;
    public string Name { get; set; }
    public string NameDisplay => Name;
    public string Unit { get; set; }
    public string UnitDisplay => Unit;
    public decimal? SpentMaterial { get; set; }
    public string SpentMaterialDisplay => SpentMaterial?.ToString();
    public bool IsUserAdded {get; set;}
}