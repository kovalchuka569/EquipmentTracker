using Models.Entities.EquipmentSheet;

namespace Data.Interfaces;

public interface IEquipmentSheetRepository
{
    /// <summary>
    /// Adds a new equipment sheet entity to the database.
    /// </summary>
    /// <param name="entity">Equipment sheet entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(EquipmentSheetEntity entity, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the columns for a specific equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>String containing the columns in JSON format</returns>
    Task<string> GetColumnsJsonAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Gets the rows for a specific equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>String containing the rows in JSON format</returns>
    Task<string> GetRowsJsonAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the columns for a specific equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="columnsJson">JSON serialized columns</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateColumnsAsync(Guid equipmentSheetId, string columnsJson, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the rows for a specific equipment sheet.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="rowsJson">JSON serialized rows</param>
    /// <param name="ct">Cancellation token</param>
    Task UpdateRowsAsync(Guid equipmentSheetId, string rowsJson, CancellationToken ct = default);
}