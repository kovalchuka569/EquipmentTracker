using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Entities.EquipmentSheet;

[Table("EquipmentSheets", Schema = "main")]
public class EquipmentSheetEntity
{
    [Key]
    public Guid Id { get; set; }
    
    public bool Deleted { get; set; }
    
    public string ColumnsJson { get; set; }

    public string RowsJson { get; set; }
}