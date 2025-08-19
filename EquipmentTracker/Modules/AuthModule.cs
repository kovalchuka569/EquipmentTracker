using Core.Models.Auth;
using Core.Services.Auth;
using Data.Repositories;
using UI.ViewModels.Auth;
using UI.Views.Authorization;

namespace UI.Modules
{
    class AuthModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
        
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterScoped<IAuthorizationService, AuthorizationService>();
            containerRegistry.RegisterScoped<AuthorizationModel>();
            containerRegistry.RegisterScoped<IUserRepository, UserRepository>();

            containerRegistry.RegisterForNavigation<ExpanderView, ExpanderViewModel>();
        }
    }
}
