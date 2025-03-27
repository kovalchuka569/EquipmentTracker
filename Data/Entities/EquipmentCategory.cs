namespace Data.Entities;

public interface EquipmentCategory
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public int? ParentId { get; set; }
}