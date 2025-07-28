using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.Table;

[Table("Rows", Schema = "main")]
public class RowEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public int Position { get; set; }
    
    public bool Deleted { get; set; }
    
    public List<CellEntity> Cells { get; set; } = new();
    
}