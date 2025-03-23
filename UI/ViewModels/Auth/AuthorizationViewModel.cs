
using Core.Models.Auth;
using Core.Services.Auth;
using Core.Services.Notifications;

namespace UI.ViewModels.Auth
{
    public class AuthorizationViewModel : BindableBase
    {
        #region Properties
        private readonly IRegionManager _regionManager;
        private readonly IAuthorizationService _authService;
        private readonly BusyIndicatorService _busyIndicatorService;

        private string _username;
        private string _password;
        private string _passFont = "pack://application:,,,/Fonts/#password";
        private string _showHideSource = "pack://application:,,,/Views/Auth/Assets/show_pass.png";

        private bool _isPasswordVisible = false;
        private bool _passwordHasError = false;
        private bool _usernameHasError = false;
        private bool _isRememberLogin;

        public string ShowHideSource
        {
            get => _showHideSource;
            set => SetProperty(ref _showHideSource, value);
        }
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }
        public bool PasswordHasError
        {
            get => _passwordHasError;
            set => SetProperty(ref _passwordHasError, value);
        }
        public bool UsernameHasError
        {
            get => _usernameHasError;
            set => SetProperty(ref _usernameHasError, value);
        }
        public bool IsRememberLogin
        {
            get => _isRememberLogin;
            set => SetProperty(ref _isRememberLogin, value);           
        }
        public string PassFont
        {
            get => _passFont;
            set => SetProperty(ref _passFont, value);
        }
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        #endregion
        #region Initialization
        public AuthorizationViewModel(IRegionManager regionManager, IAuthorizationService authService, BusyIndicatorService busyIndicatorService)
        {
            _regionManager = regionManager;
            _authService = authService;
            _busyIndicatorService = busyIndicatorService;
            

            #region Load remember login
            IsRememberLogin = Properties.Settings.Default.IsRememberLogin;
            if (IsRememberLogin)
            {
                Username = Properties.Settings.Default.RememberedLogin;
            }
                #endregion

            NavigateToExpanderCommand = new DelegateCommand(NavigateToExpander);
            AuthorizationCommand = new DelegateCommand(async () => await ExecuteLoginAsync());
            TogglePasswordVisibilityCommand = new DelegateCommand(TogglePasswordVisibility);
        }
        #endregion
        #region Commands
        public DelegateCommand NavigateToExpanderCommand { get; }
        public DelegateCommand AuthorizationCommand { get; }
        public DelegateCommand TogglePasswordVisibilityCommand { get; }
        #endregion


        #region Navigations
        private void NavigateToExpander()
        {
            _regionManager.RequestNavigate("ExpanderRegion", "ExpanderView");
        }
        #endregion
        #region ExecuteLogin
        private async Task ExecuteLoginAsync()
        {
            _busyIndicatorService.StartBusy();
            _busyIndicatorService.VisibleBusyIndicator();
            _busyIndicatorService.DefaultAuthMessage();
            await Task.Delay(500);
            var result = await Task.Run(() => _authService.AuthenticateAsync(Username, Password));
            
            switch (result)
            {
                case AuthResult.Admin:
                    Console.WriteLine("Admin");
                    UsernameHasError = false;
                    PasswordHasError = false;
                    
                    _busyIndicatorService.SuccessAuthMessage();
                    await Task.Delay(500);
                    _regionManager.RequestNavigate("MainRegion", "NavDrawerView");
                    _busyIndicatorService.StopBusy();
                    _busyIndicatorService.HiddenBusyIndicator();
                    
                    break;
                
                case AuthResult.NoAdmin:
                    Console.WriteLine("No admin");
                    UsernameHasError = false;
                    PasswordHasError = false;
                    
                    _busyIndicatorService.SuccessAuthMessage();
                    await Task.Delay(500);
                    _regionManager.RequestNavigate("MainRegion", "NavDrawerView");
                    _busyIndicatorService.StopBusy();
                    _busyIndicatorService.HiddenBusyIndicator();
                    
                    break;
                
                case AuthResult.InvalidLogin:
                    UsernameHasError = true;
                    PasswordHasError = false;
                    
                    _busyIndicatorService.ErrorAuthMessage();
                    await Task.Delay(500);
                    _busyIndicatorService.StopBusy();
                    _busyIndicatorService.HiddenBusyIndicator();
                    
                    break;
                
                case AuthResult.InvalidPassword:
                    PasswordHasError = true;
                    UsernameHasError = false;
                    
                   _busyIndicatorService.ErrorAuthMessage();
                    await Task.Delay(500);
                    _busyIndicatorService.StopBusy();
                    _busyIndicatorService.HiddenBusyIndicator();
                    
                    break;
            }

            #region Save into settings remembered login
            if (IsRememberLogin)
            {
                Properties.Settings.Default.RememberedLogin = Username;
                Properties.Settings.Default.IsRememberLogin = true;
            }
            else
            {
                Properties.Settings.Default.RememberedLogin = string.Empty;
                Properties.Settings.Default.IsRememberLogin = false;
            }
            Properties.Settings.Default.Save();
            #endregion
        }
        #endregion
        #region Show/Hide password 
        private void TogglePasswordVisibility()
        {
            if (IsPasswordVisible) HidePass();
            else ShowPass();
        }
        private void ShowPass()
        {
            IsPasswordVisible = true;
            PassFont = "Arial";
            ShowHideSource = "pack://application:,,,/Views/Auth/Assets/hide_pass.png";
        }
        private void HidePass()
        {
            IsPasswordVisible = false;
            PassFont = "pack://application:,,,/Fonts/#password";
            ShowHideSource = "pack://application:,,,/Views/Auth/Assets/show_pass.png";
        }
        #endregion
    }
}
