using Models.EquipmentTree;
using Models.Summary;
using Models.Summary.ColumnTree;
using Models.Summary.DataGrid;
using ColumnItem = Models.Summary.DataGrid.ColumnItem;
using FileDto = Models.Summary.ColumnTree.FileDto;
using FolderDto = Models.Summary.ColumnTree.FolderDto;

namespace Data.Repositories.Summary;

public interface ISummaryRepository
{
    Task<SummaryFormat> GetSummaryFormat(int summaryId);
    Task<List<FolderDto>> GetFoldersAsync();
    Task<List<FileDto>> GetEquipmentFilesAsync();
    Task<List<ColumnDto>> GetColumnsForTreeAsync(int tableId);
    
    // Tree
    Task<List<int>> GetEquipmentSelectedColumnsIds(int summaryId);
    Task DeleteEquipmentSummaryColumnAsync (List<int> columnIdsForDelete, int summaryId);
    Task InsertEquipmentSummaryColumnAsync (List<int> columnIdIdsForInsert, int summaryId, CancellationToken ct = default);
    
    // Equipment
    Task<List<ReportColumnMetadata>> GetEquipmentReportColumnsMetadata(int summaryId, CancellationToken ct = default);
    Task UpdateEquipmentSummaryMergedStatus(int summaryId, int? mergedWith, bool userAcceptedMerge, bool isMergedDecisionMade);
    Task <List<RawDataEntry>> GetRawDataForEquipmentsAsync (List<int> tableIds, CancellationToken ct = default);
}