using Prism.Commands;
using Syncfusion.UI.Xaml.Diagram.Stencil;

namespace Models.Equipment;

public class ColumnItem
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public ColumnSettings Settings { get; set; }
}