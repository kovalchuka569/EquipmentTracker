using Models.Entities.Table;

namespace Data.Repositories.Interfaces;

public interface ICellsRepository
{
    /// <summary>
    /// Get cell entity by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of cell entities</returns>
    public Task<CellEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Add new cell
    /// </summary>
    /// <param name="cell">Cell entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task AddAsync(CellEntity cell, CancellationToken ct = default);
    
    /// <summary>
    /// Update cell
    /// </summary>
    /// <param name="cell">Cell entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateAsync(CellEntity cell, CancellationToken ct = default);

    /// <summary>
    /// Update delete status of cell
    /// </summary>
    /// <param name="id">Cell id</param>
    /// <param name="deleted">New delete status</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct = default);
}