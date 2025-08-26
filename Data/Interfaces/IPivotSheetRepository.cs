using Models.Entities.PivotSheet;

namespace Data.Interfaces;

public interface IPivotSheetRepository
{
    /// <summary>
    /// Gets a summary sheet by their Id.
    /// </summary>
    /// <param name="summarySheetId">Summary sheet Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Summary sheet entity</returns>
    Task<PivotSheetEntity> GetByIdAsync (Guid summarySheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new summary sheet entity to the database.
    /// </summary>
    /// <param name="pivotSheetEntity">Summary sheet entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(PivotSheetEntity pivotSheetEntity, CancellationToken ct = default);
}