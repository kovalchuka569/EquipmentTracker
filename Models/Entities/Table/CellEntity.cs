using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.Table;

[Table("Cells", Schema = "main")]
public class CellEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public Guid RowId { get; set; }
    
    public Guid ColumnId { get; set; }
    
    public string Value { get; set; } = string.Empty;
    
    public bool Deleted { get; set; }
    
    public ColumnEntity ColumnEntity { get; set; }
    
    public RowEntity RowEnity { get; set; }
}