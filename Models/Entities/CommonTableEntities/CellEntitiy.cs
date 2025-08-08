using System.ComponentModel.DataAnnotations;

namespace Models.Entities.CommonTableEntities;

public class CellEntitiy
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RowId { get; set; }
    
    public bool Deleted { get; set; }
    
    public string MapingName { get; set; }
    
    public object Value { get; set; }
    
    public RowEntity Row { get; set; }
}