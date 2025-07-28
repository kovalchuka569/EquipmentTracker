using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.Table;

namespace Models.Entities.EquipmentSheet;

[Table("EquipmentColumns", Schema = "main")]
public class EquipmentColumnEntity
{
    public Guid EquipmentSheetId { get; set; }
    public Guid ColumnId { get; set; }
    public EquipmentSheetEntity EquipmentSheet { get; set; }
    public ColumnEntity ColumnEntity { get; set; }
}