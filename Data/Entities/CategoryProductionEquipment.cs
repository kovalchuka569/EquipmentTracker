namespace Data.Entities;

public class CategoryProductionEquipment
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public int? ParentId { get; set; }
}