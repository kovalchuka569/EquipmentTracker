namespace Models.Dialogs;

public class ExcelImportConfigurationResult
{
    /// <summary>
    /// Selected .xls file path.
    /// </summary>
    public string SelectedFilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected Excel sheet name.
    /// </summary>
    public string SelectedSheetName { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected headers range.
    /// </summary>
    public string HeadersRange { get; set; } = string.Empty;
    
    /// <summary>
    /// Selected row range start.
    /// </summary>
    public int RowRangeStart { get; set; }
    
    /// <summary>
    /// Selected row range end.
    /// </summary>
    public int RowRangeEnd { get; set; }
}