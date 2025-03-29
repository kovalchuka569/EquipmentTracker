namespace Data.Entities;

public class CategoryOfficeTechnique : EquipmentCategory
{
    public int Id { get; set; }
    public string CategoryName { get; set; }
    public int? ParentId { get; set; }
    public bool IsFinal { get; set; }
}