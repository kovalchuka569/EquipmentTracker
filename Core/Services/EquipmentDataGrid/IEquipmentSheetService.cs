using System.Collections.ObjectModel;
using Models.Equipment;
using Models.Equipment.ColumnSettings;

namespace Core.Services.EquipmentDataGrid;

public interface IEquipmentSheetService
{
    /// <summary>
    /// Get columns collection by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of column items</returns>
    Task<ObservableCollection<ColumnItem>> GetColumnsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Get rows collection by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Collection of row items</returns>
    Task<ObservableCollection<RowItem>> GetRowsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Insert a new row in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="rowItem">Row item</param>
    /// <param name="ct">Cancellation token</param>
    Task InsertRowAsync(Guid equipmentSheetId, RowItem rowItem, CancellationToken ct = default);
    
    /// <summary>
    /// Insert a new column in the equipment sheet.
    /// </summary>
    /// <param name="columnItem">Column item</param>
    /// <param name="ct">Cancellation token</param>
    Task InsertColumnAsync(ColumnItem columnItem, CancellationToken ct = default);
    
    /// <summary>
    /// Update a row in the equipment sheet.
    /// </summary>
    /// <param name="rowItem">Row item</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateRowAsync(RowItem rowItem, CancellationToken ct = default);

    /// <summary>
    /// Remove a row from equipment sheet.
    /// </summary>
    /// <param name="rowId">Row id</param>
    /// <param name="ct">Cancellation token</param>
    Task RemoveRowAsync(Guid rowId, CancellationToken ct = default);
    
    Task UpdateColumnAsync(ColumnItem column);
    
    Task UpdateColumnPositionAsync(Dictionary<Guid, int> columnPosition, Guid tableId);
    
    Task UpdateColumnWidthAsync(Dictionary<Guid, double> columnWidths, Guid tableId);
    
    Task<Guid> AddColumnAsync(ColumnSettingsDisplayModel column, Guid tableId);
    Task<List<Guid>> InsertRowsAsync(Guid tableId, List<(Guid position, Dictionary<string, object?> data)> rows, CancellationToken ct = default );
    Task RemoveItemsAsync (List<Guid> rowsId, CancellationToken ct = default);
    Task RemoveColumnAsync(Guid columnId, CancellationToken ct = default);

    ValueTask DisposeAsync();

}