namespace Data.Entities.MainTree;

public class PivotFileEntity : MainTreeItemEntity
{
    public Guid? PivotSheetId { get; set; }
    
    public PivotSheetEntity? PivotSheet { get; set; }
}