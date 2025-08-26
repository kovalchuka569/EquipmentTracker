
using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.PivotSheet;

namespace Models.Entities.FileSystem;

public class PivotFileEntity : FileSystemItemEntity
{
    public Guid? PivotSheetId { get; set; }
    
    [ForeignKey(nameof(PivotSheetId))]
    public PivotSheetEntity? PivotSheet { get; set; }
}