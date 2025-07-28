using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.Table;

namespace Models.Entities.SummarySheet;

[Table("SummaryRows", Schema = "main")]
public class SummaryRowEntity
{
    public Guid SummarySheetId { get; set; }
    public Guid RowId { get; set; }
    public int Order { get; set; }
    public SummarySheetEntity SummarySheet { get; set; }
    public RowEntity RowEntity { get; set; }
}