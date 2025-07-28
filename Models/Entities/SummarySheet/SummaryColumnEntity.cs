using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.Table;

namespace Models.Entities.SummarySheet;

[Table("SummaryColumns", Schema = "main")]
public class SummaryColumnEntity
{
    public Guid SummarySheetId { get; set; }
    public Guid ColumnId { get; set; }
    public Guid MergedWith { get; set; }
    public int Order { get; set; }
    public bool UserAcceptedMerge { get; set; }
    public bool? IsMergeDecisionMade { get; set; }
    public SummarySheetEntity SummarySheet { get; set; }
    public ColumnEntity ColumnEntity { get; set; }
}