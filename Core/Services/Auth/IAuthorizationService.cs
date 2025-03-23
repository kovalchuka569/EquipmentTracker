using Core.Models.Auth;

namespace Core.Services.Auth
{
    public interface IAuthorizationService
    {
        Task<AuthResult> AuthenticateAsync(string user, string password);
    }
}
