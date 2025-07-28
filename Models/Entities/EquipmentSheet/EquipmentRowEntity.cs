using System.ComponentModel.DataAnnotations.Schema;
using Models.Entities.Table;

namespace Models.Entities.EquipmentSheet;

[Table("EquipmentRows", Schema = "main")]
public class EquipmentRowEntity
{
    public Guid RowId { get; set; }
    public Guid EquipmentSheetId { get; set; }
    public int Order { get; set; }
    public RowEntity Row { get; set; }
    public EquipmentSheetEntity EquipmentSheet { get; set; }
}