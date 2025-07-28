using Models.Entities.Table;
namespace Data.Repositories.Interfaces;

public interface IColumnRepository
{
    /// <summary>
    /// Get column entity by id
    /// </summary>
    /// <param name="id">Id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of column entities</returns>
    public Task<ColumnEntity> GetByIdAsync(Guid id, CancellationToken ct = default);
    
    /// <summary>
    /// Add new column
    /// </summary>
    /// <param name="column">Column entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task AddAsync(ColumnEntity column, CancellationToken ct = default);
    
    /// <summary>
    /// Update column
    /// </summary>
    /// <param name="column">Column entity</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateAsync(ColumnEntity  column, CancellationToken ct = default);
    
    /// <summary>
    /// Update delete status of column
    /// </summary>
    /// <param name="id">Column id</param>
    /// <param name="deleted">New delete status</param>
    /// <param name="ct">Cancellation token</param>
    public Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct = default);
}