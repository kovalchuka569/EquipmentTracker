namespace Models.Common.Table;

public class RowModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public int Position { get; set; }
    
    public bool Deleted { get; set; }
    
    public List<CellModel> Cells { get; set; } = new();
    
}