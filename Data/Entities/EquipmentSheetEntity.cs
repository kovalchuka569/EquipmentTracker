namespace Data.Entities;

public class EquipmentSheetEntity
{
    public Guid Id { get; set; }
    
    public bool HasMarkedForDeleteColumns { get; set; }
    
    public bool HasMarkedForDeleteRows { get; set; }

    public string ColumnsJson { get; set; } = "[]";

    public string RowsJson { get; set; } = "[]";
}