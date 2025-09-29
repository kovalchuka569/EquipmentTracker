using Common.Logging;
using Data.UnitOfWork;

namespace Core.Services.Base;

/// <summary>
/// Provides a base class for database-related services.
/// </summary>
/// <typeparam name="T">The unit of work.</typeparam>
public abstract class DatabaseService<T> : BaseService<T>
{
    #region Constants 
    
    private const string BeginningTransactionMessage = "Beginning database transaction";
    private const string CommitingTransactionMessage = "Success, committing database transaction";
    private const string RollingBackTransactionMessage = "Error, rolling back database transaction due to error";
    
    #endregion
    
    #region Private fields
    
    protected readonly IUnitOfWork UnitOfWork;
    
    #endregion
    
    #region Constructor

    /// <summary>
    /// Initializes a new instance of the DatabaseService class with a unit of work and a logger.
    /// </summary>
    /// <param name="unitOfWork">The unit of work for managing database transactions and operations.</param>
    /// <param name="logger">The logger used for logging application events and messages.</param>
    protected DatabaseService(IUnitOfWork unitOfWork, IAppLogger<T> logger) 
        : base(logger)
    {
        UnitOfWork = unitOfWork;
    }
    
    #endregion

    #region Protected methods

    /// <summary>
    /// Executes a given action within a database transaction.
    /// </summary>
    /// <param name="action">The asynchronous action to be executed.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the work.</param>
    protected async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct = default)
    {
        try
        {
            Logger.LogDebug(BeginningTransactionMessage);
            await UnitOfWork.BeginTransactionAsync(ct);
            
            await action();
            
            Logger.LogDebug(CommitingTransactionMessage);
            await UnitOfWork.SaveChangesAsync(ct);
            await UnitOfWork.CommitTransactionAsync(ct);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, RollingBackTransactionMessage);
            await UnitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    #endregion
}