using System.Windows;
using System.Windows.Media;
using Color = System.Windows.Media.Color;

namespace Models.Equipment;


public class ColumnSettings
{
    public ColumnDataType DataType { get; set; }
    public string HeaderText { get; set; }
    public string MappingName { get; set; }
    public bool IsReadOnly { get; set; }
    public bool IsUnique { get; set; }
    public bool IsRequired { get; set; }
    public bool AllowSorting { get; set; }
    public bool AllowFiltering { get; set; }
    public bool AllowGrouping { get; set; }
    public bool IsPinned { get; set; }
    public int ColumnPosition { get; set; }
    public double ColumnWidth { get; set; }
    public Color HeaderBackground { get; set; }
    public Color HeaderForeground { get; set; }
    public Color HeaderBorderColor { get; set; }
    public string HeaderFontFamily { get; set; }
    public double HeaderFontSize { get; set; }
    public FontWeight HeaderFontWeight { get; set; }
    public VerticalAlignment HeaderVerticalAlignment { get; set; }
    public HorizontalAlignment HeaderHorizontalAlignment { get; set; }
    public Thickness HeaderBorderThickness { get; set; }
    
    public object SpecificSettings { get; set; }
}