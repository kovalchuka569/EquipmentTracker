namespace Data.Entities;

public class PivotSheetEntity
{
    public Guid Id { get; set; }
    
    public bool Deleted { get; set; }

    public string ColumnsJson { get; set; } = "[]";

    public string RowsJson { get; set; } = "[]";
}