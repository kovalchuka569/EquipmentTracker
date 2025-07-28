using Models.Entities.SummarySheet;

namespace Data.Repositories.Interfaces.SummarySheet;

public interface ISummarySheetColumnsRepository
{
    /// <summary>
    /// Gets a list of summary columns by summary Id.
    /// </summary>
    /// <param name="summaryId">Summary id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of summary columns</returns>
    Task<List<SummaryColumnEntity>> GetListBySummarySheetIdAsync (Guid summaryId, CancellationToken ct = default);
    
    /// <summary>
    /// Adds a new summary column entity to the database.
    /// </summary>
    /// <param name="entity">Summary column entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(SummaryColumnEntity entity, CancellationToken ct = default);
}