using Common.Enums;
using Data.Entities;

namespace Data.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Adds a new user to the database.
    /// </summary>
    /// <param name="user">User entity to be added.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddAsync(UserEntity user, CancellationToken ct = default);
    
    /// <summary>
    /// Updates the status of an existing user.
    /// </summary>
    /// <param name="userId">User ID.</param>
    /// <param name="status">New status to set for the user.</param>
    /// <param name="ct">Cancellation token.</param>
    Task ChangeUserStatusAsync(Guid userId, UserStatus status, CancellationToken ct = default);
    
    Task<bool> LoginExistsAsync(string login, CancellationToken ct = default);
    
    /// <summary>
    /// Gets password hash by login.
    /// </summary>
    /// <param name="login">User login.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>UserEntity</c> if existing.<br/>
    /// <c>NULL</c> if user with this login does not exist.
    /// </returns>>
    Task<UserEntity?> GetUserEntityAsync(string login, CancellationToken ct = default);
    
    /// <summary>
    /// Gets number of users awaiting confirmation.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Number of users awaiting confirmation.</returns>
    Task<int> GetAwaitConfirmationUsersCountAsync(CancellationToken ct = default);
    
    /// <summary>
    /// Gets all await confirmation user entities.
    /// </summary>
    /// <param name="status">
    /// User status. <c>null</c> — returns all users; <br/>
    /// otherwise filters by the specified status.
    /// </param>
    /// <param name="ct">
    /// Cancellation token. Default is <see cref="CancellationToken.None"/>.
    /// </param>
    /// <returns>List of all await confirmation users entities.</returns>
    Task<List<UserEntity>> GetAllAsync(UserStatus? status = null, CancellationToken ct = default);
}