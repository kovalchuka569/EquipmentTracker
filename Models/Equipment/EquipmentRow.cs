using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Models.Equipment;

public class EquipmentRow
{
    public int Id { get; set; }
    public ExpandoObject Data { get; set; } = new();
    
}