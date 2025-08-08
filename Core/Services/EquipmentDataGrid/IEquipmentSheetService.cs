using Models.Common.Table;

namespace Core.Services.EquipmentDataGrid;

public interface IEquipmentSheetService
{
    /// <summary>
    /// Get only not deleted columns list by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of column models</returns>
    Task<List<ColumnModel>> GetActiveColumnsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Get only not deleted rows list by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of row models</returns>
    Task<List<RowModel>> GetActiveRowsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Insert a new row in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="newRowModel">New row model.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of new id for row and id for its cells </returns>
    Task InsertRowAsync(Guid equipmentSheetId, RowModel newRowModel, CancellationToken ct = default);
    
    /// <summary>
    /// Insert a new column in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="columnModel">Column model</param>
    /// <param name="newCellModels">New cells for existing rows.</param>
    /// <param name="ct">Cancellation token</param>
    Task InsertColumnAsync(Guid equipmentSheetId, ColumnModel columnModel, List<CellModel> newCellModels, CancellationToken ct = default);
    
    /// <summary>
    /// Update a column in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnId">Updated column ID.</param>
    /// <param name="updatedColumn">Updated column model.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnAsync(Guid equipmentSheetId, Guid columnId, ColumnModel updatedColumn, CancellationToken ct = default);
    
    /// <summary>
    /// Update columns positions in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnNewPositions">List of column IDs to new positions.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnsPositionsAsync(Guid equipmentSheetId, Dictionary<Guid, int> columnNewPositions, CancellationToken ct = default);
    
    /// <summary>
    /// Update a row in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="rowId">Row for update ID.</param>
    /// <param name="updatedRowModel">Updated row model.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRowAsync(Guid equipmentSheetId, Guid rowId, RowModel updatedRowModel, CancellationToken ct = default);

    /// <summary>
    /// Updates rows multiple times.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="rowModels">List of row models.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRowsAsync(Guid equipmentSheetId, List<RowModel> rowModels, CancellationToken ct = default);
    
    /// <summary>
    /// Update a cell in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="cellId">Updating cell ID.</param>
    /// <param name="newValue">New value</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateCellValueAsync(Guid equipmentSheetId, Guid cellId, object newValue, CancellationToken ct = default);
    
    /// <summary>
    /// Remove a row from equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="rowId">Removed row ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SoftRemoveRowAsync(Guid equipmentSheetId, Guid rowId, CancellationToken ct = default);
    
    /// <summary>
    /// Remove rows from equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="rowIds">Row IDs to remove.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SoftRemoveRowsAsync(Guid equipmentSheetId, List<Guid> rowIds, CancellationToken ct = default);

    /// <summary>
    /// Soft remove all rows from equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SoftRemoveAllRowsAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Soft remove a column from equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="columnId">Column Id</param>
    /// <param name="ct">Cancellation token</param>
    Task SoftRemoveColumnAsync(Guid equipmentSheetId, Guid columnId, CancellationToken ct = default);

    /// <summary>
    /// Soft remove all cells which relate to the provided column.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnId">The column ID for which the cells will be deleted.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SoftRemoveCellsByColumnIdAsync(Guid equipmentSheetId, Guid columnId, CancellationToken ct = default);
    
    /// <summary>
    /// Dispose of the service.
    /// </summary>
    /// <returns>Value Task</returns>
    ValueTask DisposeAsync();

    
    
    Task UpdateColumnPositionAsync(Dictionary<Guid, int> columnPosition, Guid tableId);
    
    Task UpdateColumnWidthAsync(Dictionary<Guid, double> columnWidths, Guid tableId);
}