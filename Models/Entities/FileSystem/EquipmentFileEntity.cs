using Models.Entities.EquipmentSheet;
using Models.Entities.Table;

namespace Models.Entities.FileSystem;

public class EquipmentFileEntity : FileEntity
{
    public Guid EquipmentSheetId { get; set; }
    
    public EquipmentSheetEntity EquipmentSheet {get; set; }
}