using Models.Entities.EquipmentSheet;

namespace Models.Entities.FileSystem;

public class EquipmentFileEntity : FileEntity
{
    public Guid EquipmentSheetId { get; set; }
    
    public EquipmentSheetEntity EquipmentSheet {get; set; }
}