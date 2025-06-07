using Core.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Services.Auth
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly AuthorizationModel _authModel;

        public AuthorizationService(AuthorizationModel authModel)
        {
            _authModel = authModel;
        }

        public async Task<AuthResult> AuthenticateAsync(string login, string password)
        {
            var result = await _authModel.AuthenticateAsync(login, password);
            return result;
        }
    }
}
