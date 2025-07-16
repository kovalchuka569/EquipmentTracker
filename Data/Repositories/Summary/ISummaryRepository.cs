using Models.EquipmentTree;
using Models.Summary;
using Models.Summary.ColumnTree;
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
    Task<List<ColumnItem>> GetColumnItemsForEquipmentsAsync(List<int> columnIds);
    Task<List<(string, string)>> GetTableNamesForEquipmentSummaryAsync(List<int> columnIds);
    Task<List<Dictionary<string, object>>> GetDataForEquipmentsAsync(List<int> tableIds);
    Task<List<int>> GetEquipmentSelectedColumnsIds(int summaryId);
    Task DeleteEquipmentSummaryColumnAsync (List<int> columnIdsForDelete, int summaryId);
    Task InsertEquipmentSummaryColumnAsync (List<int> columnIdIdsForInsert, int summaryId);
}