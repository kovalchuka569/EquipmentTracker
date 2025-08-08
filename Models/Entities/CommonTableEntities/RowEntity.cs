using System.ComponentModel.DataAnnotations;

namespace Models.Entities.CommonTableEntities;

public class RowEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid SheetId { get; set; }
    
    public bool Deleted { get; set; }
    
    public int Order { get; set; }

    public List<CellEntitiy> Cells { get; set; } = new();
}