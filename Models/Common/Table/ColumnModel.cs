using System.Windows;
using System.Windows.Media;
using Models.Common.Table.ColumnSpecificSettings;
using Models.Common.Table.ColumnValidationRules;
using Models.Equipment;
using Color = System.Windows.Media.Color;

namespace Models.Common.Table;

public class ColumnModel
{
    public Guid Id { get; set; }
    
    public ColumnDataType DataType { get; set; }
    
    public string HeaderText { get; set; }
    
    public string MappingName { get; set; }
    
    public bool SoftDeleted { get; set; }
    
    public int Order { get; set; }
    
    public bool IsReadOnly { get; set; }
    
    public bool IsFrozen { get; set; }
    
    public bool AllowSorting { get; set; }
    
    public bool AllowFiltering { get; set; }
    
    public bool AllowGrouping { get; set; }
    
    public double Width { get; set; }
    
    public Color HeaderBackground { get; set; }
    
    public Color HeaderForeground { get; set; }
    
    public Color HeaderBorderColor { get; set; }
    
    public double HeaderFontSize { get; set; }
    
    public FontFamily HeaderFontFamily { get; set; }
    
    public FontWeight HeaderFontWeight { get; set; }
    
    public VerticalAlignment HeaderVerticalAlignment { get; set; }
    
    public HorizontalAlignment HeaderHorizontalAlignment { get; set; }
    
    public Thickness HeaderBorderThickness { get; set; }
    
    public IColumnSpecificSettings SpecificSettings { get; set; }
    
    public IColumnValidationRules ValidationRules { get; set; }
}