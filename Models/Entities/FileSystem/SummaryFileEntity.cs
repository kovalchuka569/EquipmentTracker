using Models.Entities.SummarySheet;

namespace Models.Entities.FileSystem;

public class SummaryFileEntity : FileEntity
{
    public Guid SummarySheetId { get; set; }
    
    public SummarySheetEntity SummarySheet {get; set; } 
}