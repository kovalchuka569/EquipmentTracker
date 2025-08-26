using Data.Interfaces;

namespace Data.UnitOfWork;

public interface IUnitOfWork
{
    public IFileSystemRepository FileSystemRepository { get; }

    public IEquipmentSheetRepository EquipmentSheetRepository { get; }
    
    public IPivotSheetRepository PivotSheetRepository { get; }
    
    /// <summary>
    /// Executes the specified asynchronous action within a database transaction. 
    /// Begins a transaction, runs the action, and commits the transaction if successful. 
    /// If an exception occurs, the transaction is rolled back and the exception is rethrown.
    /// </summary>
    /// <param name="action">The asynchronous action to execute within the transaction.</param>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default);
    
    /// <summary>
    /// Ensures that the database context is properly initialized and ready for read operations.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task EnsureInitializedForReadAsync(CancellationToken ct = default);

    /// <summary>
    /// Begins a new database transaction.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commits the current database transaction and returns the number of affected rows.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that returns the number of affected rows.</returns>
    Task<int> CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Rolls back the current database transaction.
    /// </summary>
    /// <param name="ct">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RollbackAsync(CancellationToken ct = default);

    /// <summary>
    /// Asynchronously disposes the database context and releases any unmanaged resources.
    /// </summary>
    /// <returns>A <see cref="ValueTask"/> representing the asynchronous dispose operation.</returns>
    ValueTask DisposeAsync();
}
