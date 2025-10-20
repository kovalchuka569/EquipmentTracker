using Models.Users;

namespace Core.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// Adds a new user.
    /// </summary>
    /// <param name="user">New user.</param>
    /// <param name="ct">Cancellation token.</param>
    Task AddUserAsync(User user, CancellationToken ct = default);
    
    /// <summary>
    /// Check if such a login exists.
    /// </summary>
    /// <param name="login">User login.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>true</c> if the login is existing.<br/>
    /// <c>false</c> if the login is valid.
    /// </returns>
    Task<bool> IsLoginExistAsync(string login, CancellationToken ct = default);
    
    /// <summary>
    /// Gets user by login.
    /// </summary>
    /// <param name="login">User login.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// <c>User</c> if login exist.<br/>
    /// <c>NULL</c> if user does not exist.
    /// </returns>>
    Task<User?> GetUserAsync(string login, CancellationToken ct = default);
}