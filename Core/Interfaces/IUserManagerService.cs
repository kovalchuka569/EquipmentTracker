using Models.Users;

namespace Core.Interfaces;

public interface IUserManagerService
{
    
    /// <summary>
    /// Gets active users.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of active users.</returns>
    Task<List<User>> GetActiveUsersAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets number of users awaiting confirmation.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of users awaiting confirmation.</returns>
    Task<int> GetAwaitConfirmationUsersCountAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets all await confirmation users.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of all await confirmation users.</returns>
    Task<List<User>> GetAwaitConfirmationUsersAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Approves the user into the system.
    /// </summary>
    /// <param name="userId">User ID who approved.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ApproveQueryUserAsync(Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Rejects the user from await confirmation query.
    /// </summary>
    /// <param name="userId">User ID who rejected.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RejectQueryUserAsync(Guid userId, CancellationToken ct = default);
}