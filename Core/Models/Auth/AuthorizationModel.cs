using Data.Repositories;

namespace Core.Models.Auth
{
    public class AuthorizationModel
    {
        private readonly IUserRepository _userRepository;

        public AuthorizationModel(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task <AuthResult> AuthenticateAsync(string login, string password)
        {
            var user = await _userRepository.GetUserByLoginAsync(login);
            if (user == null) return AuthResult.InvalidLogin;
            if (user.Password != password) return AuthResult.InvalidPassword;
            return user.IsAdmin ? AuthResult.Admin : AuthResult.NoAdmin;
        }
    }
}
