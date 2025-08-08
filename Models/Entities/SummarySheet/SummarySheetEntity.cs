using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.SummarySheet;

[Table("SummarySheets", Schema = "main")]
public class SummarySheetEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public bool Deleted { get; set; }
    
    [Column(TypeName = "jsonb")]
    public string ColumnsJson { get; set; } = "[]";
    
    [Column(TypeName = "jsonb")]
    public string RowsJson { get; set; } = "[]";
}