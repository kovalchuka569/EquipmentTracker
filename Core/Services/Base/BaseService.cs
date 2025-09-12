using System.Diagnostics;
using Common.Logging;

namespace Core.Services.Base;

/// <summary>
/// Provides a base class for services that handle a specific type of entity.
/// </summary>
/// <typeparam name="T">The type of the entity handled by the service.</typeparam>
public abstract class BaseService<T>
{
    #region Constatnts
    
    private const string StartingOperationLog = "Starting: {Operation}";
    private const string CompletedOperationWithTimeLog = "Completed: {Operation} in {Ms}ms";
    private const string FailedOperationWithTimeLog = "Failed: {Operation} after {Ms}ms";
    
    #endregion
    
    #region Private fields
    
    protected readonly IAppLogger<T> Logger;
    
    #endregion

    #region Constructor
    
    /// <summary>
    /// Initializes a new instance of the BaseService class with a logger.
    /// </summary>
    /// <param name="logger">The application logger used for logging messages and events for the service.</param>
    protected BaseService(IAppLogger<T> logger)
    {
        Logger = logger;
    }
    
    #endregion

    #region Protected methods

        /// <summary>
    /// Executes a given asynchronous action while logging its start, completion, and any failures, including the time taken for the operation.
    /// </summary>
    /// <param name="action">The asynchronous action to be executed.</param>
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the work.</param>
    protected async Task ExecuteInLoggerAsync(Func<Task> action, string operationName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.LogInformation(StartingOperationLog, operationName);

        try
        {
            await action();
            stopwatch.Stop();
            Logger.LogInformation(CompletedOperationWithTimeLog, operationName, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            Logger.LogError(e,FailedOperationWithTimeLog, operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
    
    /// <summary>
    /// Executes a given asynchronous action that returns a result, while logging its start, completion, and any failures, including the time taken for the operation.
    /// </summary>
    /// <typeparam name="TResult">The type of the result returned by the action.</typeparam>
    /// <param name="action">The asynchronous action to be executed.</param>
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="ct">A cancellation token that can be used to cancel the work.</param>
    /// <returns>A task that represents the asynchronous operation, containing the result from the executed action.</returns>
    protected async Task<TResult> ExecuteInLoggerAsync<TResult>(Func<Task<TResult>> action, string operationName, CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        Logger.LogInformation(StartingOperationLog, operationName);

        try
        {
            var result = await action();
            stopwatch.Stop();
            Logger.LogInformation(CompletedOperationWithTimeLog, operationName, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            Logger.LogError(e,FailedOperationWithTimeLog, operationName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    #endregion
}