using Models.Entities.SummarySheet;

namespace Data.Repositories.Interfaces.SummarySheet;

public interface ISummarySheetsRepository
{
    /// <summary>
    /// Gets a summary sheet by their Id.
    /// </summary>
    /// <param name="summarySheetId">Summary sheet Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Summary sheet entity</returns>
    Task<SummarySheetEntity> GetByIdAsync (Guid summarySheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new summary sheet entity to the database.
    /// </summary>
    /// <param name="summarySheetEntity">Summary sheet entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(SummarySheetEntity summarySheetEntity, CancellationToken ct = default);
}