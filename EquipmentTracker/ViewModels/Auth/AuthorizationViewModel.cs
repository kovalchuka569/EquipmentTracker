using Core.Models.Auth;
using Core.Services.Auth;
using Core.Services.Notifications;
using EquipmentTracker.Properties;
using Microsoft.EntityFrameworkCore;
using Prism.Mvvm;
using Prism.Commands;
using DbContext = Data.AppDbContext.DbContext;

namespace UI.ViewModels.Auth
{
    /// <summary>
    /// ViewModel responsible for handling user authentication and managing the login process.
    /// </summary>
    public class AuthorizationViewModel : BindableBase, INavigationAware
    {
        #region Properties
        private readonly IRegionManager _regionManager;
        private readonly BusyIndicatorService _busyIndicatorService;
        private readonly DbContext _dbContext;
        

        private string _username;
        private string _password;
        private string _passFont = "pack://application:,,,/Resources/Fonts/#password";
        private string _showHideSource = "pack://application:,,,/Resources/Assets/show_pass.png";

        private bool _isPasswordVisible = false;
        private bool _passwordHasError = false;
        private bool _usernameHasError = false;
        private bool _isRememberLogin;

        /// <summary>
        /// Gets or sets the source of the image for showing/hiding the password.
        /// </summary>
        public string ShowHideSource
        {
            get => _showHideSource;
            set => SetProperty(ref _showHideSource, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the password is visible.
        /// </summary>
        public bool IsPasswordVisible
        {
            get => _isPasswordVisible;
            set => SetProperty(ref _isPasswordVisible, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the password has an error.
        /// </summary>
        public bool PasswordHasError
        {
            get => _passwordHasError;
            set => SetProperty(ref _passwordHasError, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the username has an error.
        /// </summary>
        public bool UsernameHasError
        {
            get => _usernameHasError;
            set => SetProperty(ref _usernameHasError, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the login credentials should be remembered.
        /// </summary>
        public bool IsRememberLogin
        {
            get => _isRememberLogin;
            set => SetProperty(ref _isRememberLogin, value);           
        }

        /// <summary>
        /// Gets or sets the font style for the password field.
        /// </summary>
        public string PassFont
        {
            get => _passFont;
            set => SetProperty(ref _passFont, value);
        }

        /// <summary>
        /// Gets or sets the username for authentication.
        /// </summary>
        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        /// <summary>
        /// Gets or sets the password for authentication.
        /// </summary>
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationViewModel"/> class.
        /// </summary>
        /// <param name="regionManager">The region manager for navigating between views.</param>
        /// <param name="authService">The service responsible for user authentication.</param>
        /// <param name="busyIndicatorService">The service for showing busy indicators during long operations.</param>
        public AuthorizationViewModel(IRegionManager regionManager, BusyIndicatorService busyIndicatorService, DbContext dbContext)
        {
            try
            {
                _regionManager = regionManager;
                _busyIndicatorService = busyIndicatorService;
                _dbContext = dbContext;

                IsRememberLogin = Settings.Default.IsRememberLogin;
                if (IsRememberLogin)
                {
                    Username = Settings.Default.RememberedLogin;
                }

                NavigateToExpanderCommand = new DelegateCommand(NavigateToExpander);
                AuthorizationCommand = new DelegateCommand(async () => await ExecuteLoginAsync());
                TogglePasswordVisibilityCommand = new DelegateCommand(TogglePasswordVisibility);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in AuthorizationViewModel constructor: {ex.Message}");
                throw;
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Command that navigates to the expander view.
        /// </summary>
        public DelegateCommand NavigateToExpanderCommand { get; }

        /// <summary>
        /// Command that triggers the authentication process.
        /// </summary>
        public DelegateCommand AuthorizationCommand { get; private set; }

        /// <summary>
        /// Command that toggles the visibility of the password.
        /// </summary>
        public DelegateCommand TogglePasswordVisibilityCommand { get; }
        #endregion

        #region Navigations
        /// <summary>
        /// Navigates to the ExpanderView.
        /// </summary>
        private void NavigateToExpander()
        {
            _regionManager.RequestNavigate("ExpanderRegion", "ExpanderView");
        }
        #endregion
        

        #region ExecuteLogin
        /// <summary>
        /// Executes the login process asynchronously by authenticating the user.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task ExecuteLoginAsync()
        {
            _busyIndicatorService.StartBusy();
            _busyIndicatorService.VisibleBusyIndicator();
            _busyIndicatorService.DefaultAuthMessage();

            await Task.Delay(1000);
            
            try 
            {
                Settings.Default.Reload();
                // Check user by username
                var user = await _dbContext.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Login == Username);

                AuthResult result;
                if (user == null)
                    result = AuthResult.InvalidLogin;
                else if (user.Password != Password)
                    result = AuthResult.InvalidPassword;
                else
                    result = user.IsAdmin ? AuthResult.Admin : AuthResult.NoAdmin;

                switch (result)
                {
                    case AuthResult.Admin:
                        UsernameHasError = false;
                        PasswordHasError = false;
                        _busyIndicatorService.SuccessAuthMessage();
                        await Task.Delay(500);
                        _regionManager.RequestNavigate("MainRegion", "NavDrawerView");
                        _busyIndicatorService.StopBusy();
                        _busyIndicatorService.HiddenBusyIndicator();
                        break;
                    case AuthResult.NoAdmin:
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
                
                // Save settings
                if (IsRememberLogin)
                {
                    Settings.Default.RememberedLogin = Username;
                    Settings.Default.IsRememberLogin = true;
                }
                else
                {
                    Settings.Default.RememberedLogin = string.Empty;
                    Settings.Default.IsRememberLogin = false;
                }
                Settings.Default.Save();
            }
            catch (Exception ex)
            {
                _busyIndicatorService.ErrorAuthMessage();
                await Task.Delay(500);
                _busyIndicatorService.StopBusy();
                _busyIndicatorService.HiddenBusyIndicator();
            }
        }
        #endregion

        #region Show/Hide password 
        /// <summary>
        /// Toggles the visibility of the password.
        /// </summary>
        private void TogglePasswordVisibility()
        {
            if (IsPasswordVisible) HidePass();
            else ShowPass();
        }

        /// <summary>
        /// Makes the password visible.
        /// </summary>
        private void ShowPass()
        {
            IsPasswordVisible = true;
            PassFont = "Arial";
            ShowHideSource = "pack://application:,,,/Resources/Assets/hide_pass.png";
        }

        /// <summary>
        /// Hides the password.
        /// </summary>
        private void HidePass()
        {
            IsPasswordVisible = false;
            PassFont = "pack://application:,,,/Resources/Fonts/#password";
            ShowHideSource = "pack://application:,,,/Resources/Assets/show_pass.png";
        }
        #endregion
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
