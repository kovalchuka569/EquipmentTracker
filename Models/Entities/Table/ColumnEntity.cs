using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Models.Table;

namespace Models.Entities.Table;

[Table("Columns", Schema = "main")]
public class ColumnEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public bool Deleted { get; set; }
    
    public ColumnSettings Settings {get; set; }
    
    public List<CellEntity> Cells { get; set; } = new();
    
}