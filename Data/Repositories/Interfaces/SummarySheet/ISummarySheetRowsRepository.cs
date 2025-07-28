using Models.Entities.SummarySheet;

namespace Data.Repositories.Interfaces.SummarySheet;

public interface ISummarySheetRowsRepository
{
    /// <summary>
    /// Gets a list of summary rows entities by summary sheet Id.
    /// </summary>
    /// <param name="summarySheetId">Summary sheet Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of summary rows entities</returns>
    Task<List<SummaryRowEntity>> GetListBySummarySheetIdAsync (Guid summarySheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new summary row entity to the database.
    /// </summary>
    /// <param name="summaryRowEntity">Summary row entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(SummaryRowEntity summaryRowEntity, CancellationToken ct = default);
}