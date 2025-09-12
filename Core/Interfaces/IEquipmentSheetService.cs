using Models.Common.Table;
using Models.Common.Table.ColumnProperties;

namespace Core.Interfaces;

public interface IEquipmentSheetService
{
    /// <summary>
    /// Get column props list by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of column models</returns>
    Task<List<BaseColumnProperties>> GetColumnPropsByEquipmentSheetIdAsync(Guid equipmentSheetId,
        CancellationToken ct = default);

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
    /// Insert a new rows in equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="newRowsModels">List of new rows models.</param>
    /// <param name="ct">Cancellation token.</param>
    Task InsertRowsAsync(Guid equipmentSheetId, List<RowModel> newRowsModels, CancellationToken ct = default);

    /// <summary>
    /// Add a new columns props in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="columnProps">List of new columns properties</param>
    /// <param name="newCellModels">New cells for existing rows.</param>
    /// <param name="ct">Cancellation token</param>
    Task AddColumnPropsAsync(Guid equipmentSheetId, List<BaseColumnProperties> columnProps,
        List<CellModel> newCellModels, CancellationToken ct = default);

    /// <summary>
    /// Update a column props.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="updatedColumnProps">List of updated column properties.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnPropsAsync(Guid equipmentSheetId, List<BaseColumnProperties> updatedColumnProps,
        CancellationToken ct = default);

    /// <summary>
    /// Update columns positions in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnNewPositions">List of column IDs to new positions.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnsPositionsAsync(Guid equipmentSheetId, Dictionary<Guid, int> columnNewPositions,
        CancellationToken ct = default);

    /// <summary>
    /// Update a column width in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnId">Column ID.</param>
    /// <param name="newWidth">New column width.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnWidthAsync(Guid equipmentSheetId, Guid columnId, double newWidth, CancellationToken ct = default);

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
    /// <param name="sortByPosition">Sort by position (default false).</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRowsAsync(Guid equipmentSheetId, List<RowModel> rowModels, bool sortByPosition = false,
        CancellationToken ct = default);

    /// <summary>
    /// Update flag "IsMarkedForDelete" for rows asynchronously and based on the number of rows marked for deletion,
    /// updates the HasMarkedForDeleteRows flag of the equipment sheet
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="rowIds">Row IDs for updating flag "IsMarkedForDelete".</param>
    /// <param name="isMarkedForDelete">New flag value for rows.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateRowsMarkedForDeleteAsync(Guid equipmentSheetId, List<Guid> rowIds, bool isMarkedForDelete, CancellationToken ct = default);

    /// <summary>
    /// Update flag "IsMarkedForDelete" for columns asynchronously and based on the number of rows marked for deletion,
    /// updates the HasMarkedForDeleteColumns flag of the equipment sheet
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="columnIds">Column IDs for updating flag "IsMarkedForDelete".</param>
    /// <param name="isMarkedForDelete">New flag value for columns.</param>
    /// <param name="ct">Cancellation token.</param>
    Task UpdateColumnsMarkedForDeleteAsync(Guid equipmentSheetId, List<Guid> columnIds, bool isMarkedForDelete, CancellationToken ct = default);
    
    /// <summary>
    /// Update a cell in the equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet ID.</param>
    /// <param name="cellId">Updating cell ID.</param>
    /// <param name="newValue">New value</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateCellValueAsync(Guid equipmentSheetId, Guid cellId, object newValue, CancellationToken ct = default);
}
    