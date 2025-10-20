using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Enums;
using Core.Interfaces;
using JetBrains.Annotations;
using LocalSecure.Interfaces;
using LocalSecure.Managers;
using Notification.Wpf;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.DialogViewModels;
using Presentation.Views;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Navigation.Regions;
using Resources.Localization;
using Unity;

namespace Presentation.ViewModels;

public class AuthorizationViewModel : InteractiveViewModelBase
{

    #region Dependenies
    
    [Dependency]
    public required IDbConnectionService DbConnectionService = null!;
    
    [Dependency]
    public required IDbKeyService DbKeyService = null!;
    
    [Dependency]
    public required IAuthService AuthService = null!;
    
    [Dependency]
    public required NotificationManager NotificationManager = null!;

    #endregion

    #region Private fields

    private bool _isConnectedToDatabase;

    private string _login = string.Empty;
    private string _password = string.Empty;

    private bool _isRememberLogin;

    #endregion

    #region Public fields

    public bool IsConnectedToDatabase
    {
        get => _isConnectedToDatabase;
        set => SetProperty(ref _isConnectedToDatabase, value);
    }

    public string Login
    {
        get => _login;
        set
        {
            if (!SetProperty(ref _login, value))
                return;
            
            RemoveError(nameof(Login), ValidationUIMessages.InvalidLogin);
            RemoveError(nameof(Login), ValidationUIMessages.WaitingConfirmation);
                
            if (IsRememberLogin)
                RememberLogin();
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
                RemoveError(nameof(Password), ValidationUIMessages.InvalidPassword);
        }
    }

    public bool IsRememberLogin
    {
        get => _isRememberLogin;
        set
        {
            if (!SetProperty(ref _isRememberLogin, value))
                return;

            RememberLogin();
        }
    }

    #endregion

    public AuthorizationViewModel()
    {
        InitializeCommands();
        GetRememberLogin();
    }


    #region Commands management

    public AsyncDelegateCommand AuthorizationViewLoadedCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand ConnectionSetupCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand GetAccessCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand AuthorizeCommand { [UsedImplicitly] get; private set; } = null!;

    private void InitializeCommands()
    {
        AuthorizationViewLoadedCommand = new AsyncDelegateCommand(TestDatabaseConnectionAsync);
        ConnectionSetupCommand = new AsyncDelegateCommand(ShowConnectionSetupDialogAsync);
        GetAccessCommand = new AsyncDelegateCommand(OnGetAccessCommandExecuted);
        AuthorizeCommand = new AsyncDelegateCommand(OnAuthorizeCommandExecuted);
    }

    #endregion

    private async Task TestDatabaseConnectionAsync()
    {
        try
        {
            await BusyIndicatorService
                .ShowBusyIndicator()
                .In(this)
                .WithHeader(Strings.TestingDbConnection)
                .WithOverlay()
                .ExecuteAsync(async () =>
                {
                    // Gets the connection string from local key service
                    var connectionString = DbKeyService.LoadDbConnectionString();

                    // Testing this connection string
                    await DbConnectionService.TestConnectionAsync(connectionString);
                
                    // Warming up connection
                    await DbConnectionService.PreheatConnectionAsync();

                    // Shows success snackbar
                    NotificationManager.Show(Strings.SnackbarMessage_DbConnectionEstablished, NotificationType.Success);
                });

            IsConnectedToDatabase = true;
        }
        catch (Exception e)
        {
            IsConnectedToDatabase = false;
            await ShowConnectionFailedDialogAsync(e
                .Message); // Shows connection failed dialog if we have exception in test connection service method
        }
    }

    private async Task ShowConnectionSetupDialogAsync()
    {
        // Show dialog and await dialog result
        var dialogResult = await DialogService 
            .Show<ConnectionSetupViewModel>()
            .In(this)
            .WithOverlay()
            .Await();

        // Returns if dialog canceled
        if (dialogResult.Result == ButtonResult.Cancel)
            return;

        // Test connection 
        await TestDatabaseConnectionAsync();
    }

    private async Task ShowConnectionFailedDialogAsync(string connectionExceptionMessage)
    {
        // Put exception message in dialog parameters
        var dialogParameters = new DialogParameters
        {
            {
                ParameterKeys.ConnectionFailedDialogParameterExMessageKey, connectionExceptionMessage
            } 
        };

        var dialogResult = await DialogService 
            .Show<ConnectionFailedViewModel>()
            .WithParameters(dialogParameters)
            .In(this)
            .WithOverlay()
            .Await();

        // Returns if dialog result does not have parameters (dialog canceled)
        if (dialogResult.Parameters.Count == 0)
            return;

        // Gets connection failed dialog result from parameters
        var connectionFailedDialogResult = dialogResult.Parameters
            .GetValue<ConnectionFailedDialogResult>(ParameterKeys.ConnectionFailedDialogResultKey);


        await ProcessConnectionFailedDialogResultAsync(connectionFailedDialogResult); // Process dialog result
    }

    private async Task ProcessConnectionFailedDialogResultAsync(
        ConnectionFailedDialogResult connectionFailedDialogResult)
    {
        switch (connectionFailedDialogResult)
        {
            case ConnectionFailedDialogResult.TryAgain:
                await TestDatabaseConnectionAsync(); // Test connection again
                break;
            case ConnectionFailedDialogResult.ConnectionSetup:
                await ShowConnectionSetupDialogAsync(); // Show connection setup dialog
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(connectionFailedDialogResult),
                    connectionFailedDialogResult, null);
        }
    }

    private async Task OnGetAccessCommandExecuted()
    {
        await DialogService 
            .Show<RegisterViewModel>()
            .In(this)
            .WithOverlay()
            .Await();
    }

    private async Task OnAuthorizeCommandExecuted()
    {
        var user = await AuthService.GetUserAsync(_login);

        if (user is null)
        {
            AddError(nameof(Login), ValidationUIMessages.InvalidLogin);
            return;
        }

        if (user.Status is UserStatus.AwaitingConfirmation)
        {
            AddError(nameof(Login), ValidationUIMessages.WaitingConfirmation);
            return;
        }

        if (!PasswordHasher.VerifyPassword(Password, user.PasswordHash))
        {
            AddError(nameof(Password), ValidationUIMessages.InvalidPassword);
            return;
        }

        RegionManager.RequestNavigate("MainRegion", nameof(NavDrawerView));
    }

    private void RememberLogin()
    {
        LoginSaver.SaveLogin(IsRememberLogin ? Login : string.Empty);
    }

    private void GetRememberLogin()
    {
        var login = LoginSaver.GetSavedLogin();

        if (string.IsNullOrEmpty(login))
        {
            Login = string.Empty;
            IsRememberLogin = false;
            return;
        }

        Login = login;
        IsRememberLogin = true;
    }
}
    
    