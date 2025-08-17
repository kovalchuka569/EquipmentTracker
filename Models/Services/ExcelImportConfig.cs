using Models.Equipment;

namespace Models.Services;

public class ExcelImportConfig
{
    /// <summary>
    /// Xls file path.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Excel sheet name.
    /// </summary>
    public string SheetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Headers range.
    /// </summary>
    public string HeadersRange { get; set; } = string.Empty;
    
    /// <summary>
    /// Row range start.
    /// </summary>
    public int RowRangeStart { get; set; }
    
    /// <summary>
    /// Row range end.
    /// </summary>
    public int RowRangeEnd { get; set; }
    
    /// <summary>
    /// Available columns in data grid.
    /// </summary>
    public List<(string HeaderText, string MappingName, ColumnDataType ColumnType)> AvailableColumns { get; set; } = new();
}