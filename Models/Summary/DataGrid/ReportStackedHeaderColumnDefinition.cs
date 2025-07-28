namespace Models.Summary.DataGrid;

public class ReportStackedHeaderColumnDefinition
{
    public string HeaderText { get; set; }
    public string MappingName { get; set; }
    public List<string> ChildColumnMappingNames { get; set; } = new List<string>();
}