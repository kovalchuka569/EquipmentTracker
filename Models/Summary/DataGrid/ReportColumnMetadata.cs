using Models.Equipment;
using Models.Equipment.ColumnSettings;

namespace Models.Summary.DataGrid;

public class ReportColumnMetadata
{
    public int CustomColumnId { get; set; }
    public int? MergedIntoColumnId { get; set; } 
    public string HeaderText { get; set; } 
    public string MappingName { get; set; }
    public string EquipmentSheetName { get; set; } 
    public int TableId { get; set; }
    public int EquipmentSummaryEntryId { get; set; }
    public bool UserAcceptedMerge { get; set; }
    public bool? IsMergeDecisionMade { get; set; } 
    public ColumnSettingsDisplayModel ColumnSettings { get; set; } 
}