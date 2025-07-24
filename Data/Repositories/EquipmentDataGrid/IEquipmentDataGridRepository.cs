using Models.Equipment;

namespace Data.Repositories.EquipmentDataGrid;

public interface IEquipmentDataGridRepository
{
    Task<List<ColumnDto>> GetColumnsAsync(int tableId, CancellationToken cancellationToken = default);
    Task<List<RowItem>> GetRowsAsync(int tableId, CancellationToken cancellationToken = default);
    Task UpdateRowAsync(int rowId, int tableId, Dictionary<string, object?> updatedRowData);
    Task UpdateColumnAsync(ColumnDto column);
    Task UpdateColumnPositionAsync(Dictionary<int, int> columnPositions, int tableId);
    Task UpdateColumnWidthAsync(Dictionary<int, double> columnWidths, int tableId);
    Task<int> CreateColumnAsync(ColumnSettings column, int tableId);
    Task<int> InsertRowAsync(int tableId, int position, Dictionary<string, object?> newRowData, CancellationToken ct = default);
    Task<List<int>> InsertRowsAsync(int tableId, List<(int position, Dictionary<string, object?> data)> rows, CancellationToken ct = default );
    Task RemoveItemsAsync(List<int> rowsId, CancellationToken ct = default);
    Task RemoveColumnAsync(int columnId, CancellationToken ct = default);
}