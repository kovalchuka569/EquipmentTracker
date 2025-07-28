using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.SummarySheet;

[Table("SummarySheets", Schema = "main")]
public class SummarySheetEntity
{
    [Key]
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
    public List<SummaryColumnEntity> SummaryColumns { get; set; } = new();
    public List<SummaryRowEntity> SummaryRows { get; set; } = new();
}