namespace Models.Table;

public class ColumnSettings
{
    public string DataType { get; set; }
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
    public string HeaderBackground { get; set; }
    public string HeaderForeground { get; set; }
    public string HeaderBorderColor { get; set; }
    public string HeaderFontFamily { get; set; }
    public double HeaderFontSize { get; set; }
    public string HeaderFontWeight { get; set; }
    public string HeaderVerticalAlignment { get; set; }
    public string HeaderHorizontalAlignment { get; set; }
    public double LeftHeaderBorderThickness { get; set; }
    public double TopHeaderBorderThickness { get; set; }
    public double RightHeaderBorderThickness { get; set; }
    public double BottomHeaderBorderThickness { get; set; }
    public ColumnSpecificSettingsBase SpecificSettings { get; set; }
}