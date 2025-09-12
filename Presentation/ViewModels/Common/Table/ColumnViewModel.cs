using Models.Equipment;

namespace Presentation.ViewModels.Common.Table;

public class ColumnViewModel
{
    public string HeaderText { get; set; } = string.Empty;
    
    public string MappingName { get; set; } = string.Empty;
    
    public ColumnDataType DataType { get; set; }
}