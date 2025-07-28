namespace Models.Summary.DataGrid;

public class ReportStackedHeaderRow
{
    public string HeaderText { get; set; }
    public string MappingName { get; set; } // MappingName для StackedColumn
    public List<string> ChildColumnMappingNames { get; set; } = new List<string>();
}