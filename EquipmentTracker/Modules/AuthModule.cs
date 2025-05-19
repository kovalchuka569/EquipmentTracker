using Core.Models.Auth;
using Core.Services.Auth;
using Data.Repositories;
using Prism.Ioc;
using Prism.Modularity;
using UI.ViewModels;
using UI.ViewModels.Auth;
using UI.ViewModels.NavDrawer;
using UI.Views.Authorization;
using UI.Views.NavDrawer;

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
            containerRegistry.RegisterForNavigation<NavDrawerView, NavDrawerViewModel>();
        }
    }
}
