namespace Data.Entities.MainTree;

public class EquipmentFileEntity : MainTreeItemEntity
{
    public Guid? EquipmentSheetId { get; set; }
    
    public EquipmentSheetEntity? EquipmentSheet { get; set; }
}