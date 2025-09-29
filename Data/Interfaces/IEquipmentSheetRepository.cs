using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Models.Entities.EquipmentSheet;

namespace Data.Interfaces;

public interface IEquipmentSheetRepository
{
    /// <summary>
    /// Gets equipment sheet by ID.
    /// </summary>
    /// <param name="id">Equipment sheet ID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Equipment sheet entity.</returns>
    Task<EquipmentSheetEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new equipment sheet entity to the database.
    /// </summary>
    /// <param name="entity">Equipment sheet entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(EquipmentSheetEntity entity, CancellationToken ct = default);
    
    /// <summary>
    /// Updates all properties of the specified <see cref="EquipmentSheetEntity"/> in the database.
    /// The entity is loaded from the database, its properties are updated, and changes are saved.
    /// </summary>
    /// <param name="entity">The <see cref="EquipmentSheetEntity"/> containing updated values.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    Task UpdateAsync(EquipmentSheetEntity entity, CancellationToken ct);

    /// <summary>
    /// Updates specific fields of an <see cref="EquipmentSheetEntity"/> in the database identified by its Id.
    /// Allows partial updates without loading the entity into memory using EF Core's ExecuteUpdateAsync.
    /// </summary>
    /// <param name="id">The Id of the <see cref="EquipmentSheetEntity"/> to update.</param>
    /// <param name="updateExpression">
    /// A function defining which properties to update using <see cref="SetPropertyCalls{TEntity}"/>.
    /// </param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    Task UpdateAsync(Guid id, Expression<Func<SetPropertyCalls<EquipmentSheetEntity>, SetPropertyCalls<EquipmentSheetEntity>>> updateExpression, CancellationToken ct = default);
    
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