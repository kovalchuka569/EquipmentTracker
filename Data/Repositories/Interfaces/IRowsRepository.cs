using Models.Entities.Table;
namespace Data.Repositories.Interfaces;

public interface IRowsRepository
{
    /// <summary>
    /// Get row entity by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of rows entities</returns>
    public Task<RowEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Add new row
    /// </summary>
    /// <param name="row">Row entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task AddAsync(RowEntity row, CancellationToken ct = default);
    
    /// <summary>
    /// ### FIXME
    /// Only updating the position of the row
    /// </summary>
    /// <param name="row">Row entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateAsync(RowEntity row, CancellationToken ct = default);

    /// <summary>
    /// Update delete status of row
    /// </summary>
    /// <param name="id">Row id</param>
    /// <param name="deleted">New delete status</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct = default);
}