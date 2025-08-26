using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.PivotSheet;

[Table("PivotSheets", Schema = "main")]
public class PivotSheetEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public bool Deleted { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string ColumnsJson { get; set; } = "[]";
    
    [Column(TypeName = "jsonb")]
    public string RowsJson { get; set; } = "[]";
}