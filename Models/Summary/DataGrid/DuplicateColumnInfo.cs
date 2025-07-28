namespace Models.Summary.DataGrid;

public class DuplicateColumnInfo
{
    public ReportColumnMetadata ExistingColumn { get; set; } 
    public ReportColumnMetadata DuplicateColumn { get; set; }
}