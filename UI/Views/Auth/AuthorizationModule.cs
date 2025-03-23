using Core.Models.Auth;
using Core.Services.Auth;
using Data.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI.ViewModels.Notifications.NoifyIcon;
using UI.Views.NavDrawer;

namespace UI.Views.Auth
{
    class AuthorizationModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<IUserRepository, UserRepository>();
            containerRegistry.RegisterSingleton<IAuthorizationService, AuthorizationService>();

            containerRegistry.RegisterForNavigation<ExpanderView>();
            containerRegistry.RegisterForNavigation<NavDrawerView>();
        }
    }
}
