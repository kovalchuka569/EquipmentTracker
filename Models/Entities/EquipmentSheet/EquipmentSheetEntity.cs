using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.EquipmentSheet;

[Table("EquipmentSheets", Schema = "main")]
public class EquipmentSheetEntity
{
    [Key]
    public Guid Id { get; set; }
    public bool Deleted { get; set; }
    public List<EquipmentRowEntity> EquipmentRows { get; set; } = new();
    public List<EquipmentColumnEntity> EquipmentColumns { get; set; } = new();
}