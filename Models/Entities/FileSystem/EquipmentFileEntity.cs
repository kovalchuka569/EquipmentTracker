
using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.EquipmentSheet;

namespace Models.Entities.FileSystem;

public class EquipmentFileEntity : FileSystemItemEntity
{
    public Guid? EquipmentSheetId { get; set; }
    
    [ForeignKey(nameof(EquipmentSheetId))]
    public EquipmentSheetEntity? EquipmentSheet { get; set; }
}