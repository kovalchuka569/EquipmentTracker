using System;
using System.Threading.Tasks;
using Common.Constants;
using Common.Enums;

using Common.Logging;
using Core.Interfaces;
using Presentation.Contracts;
using JetBrains.Annotations;
using LocalDbConnectionService.Interfaces;
using Notification.Wpf;
using Presentation.Enums;
using Presentation.Interfaces;
using Prism.Commands;
using Prism.Dialogs;
using Resources.Localization;

namespace Presentation.ViewModels;

public class AuthorizationViewModel : BaseViewModel<AuthorizationViewModel>, IDialogHost, IOverlayHost, IBusyIndicatorHost
{
    
    #region Dependenies

    private readonly IDialogManager _dialogManager;
    private readonly IOverlayManager _overlayManager;
    private readonly IBusyIndicatorManager _busyIndicatorManager;
    private readonly IDbConnectionService _dbConnectionService;
    private readonly IDbKeyService _dbKeyService;

    #endregion
    
    #region Private fields
    
    private bool _isDialogOpen;
    private object? _dialogContent;
    private bool _isOverlayOpen;
    private object? _overlayContent;
    private bool _isBusy;
    private object? _busyContent;

    private bool _isConnectedToDatabase;
    
    #endregion

    #region Public fields

    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetProperty(ref _isDialogOpen, value);
    }

    public object? DialogContent
    {
        get => _dialogContent;
        set => SetProperty(ref _dialogContent, value);
    }

    public bool IsOverlayOpen
    {
        get => _isOverlayOpen;
        set => SetProperty(ref _isOverlayOpen, value);
    }

    public object? OverlayContent
    {
        get => _overlayContent;
        set => SetProperty(ref _overlayContent, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    public object? BusyContent
    {
        get => _busyContent;
        set => SetProperty(ref _busyContent, value);
    }

    public bool IsConnectedToDatabase
    {
        get => _isConnectedToDatabase;
        set => SetProperty(ref _isConnectedToDatabase, value);
    }
    
    #endregion
    
    public AuthorizationViewModel(NotificationManager notificationManager, 
        IAppLogger<AuthorizationViewModel> logger, 
        IDialogManager dialogManager, 
        IOverlayManager overlayManager,
        IBusyIndicatorManager busyIndicatorManager,
        IDbConnectionService dbConnectionService,
        IDbKeyService dbKeyService) 
        : base(notificationManager, logger)
    {
        _dialogManager = dialogManager;
        _overlayManager = overlayManager;
        _busyIndicatorManager = busyIndicatorManager;
        _dbConnectionService = dbConnectionService;
        _dbKeyService = dbKeyService;
        
        InitializeCommands();
    }


    #region Commands management

    public AsyncDelegateCommand AuthorizationViewLoadedCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand ConnectionSetupCommand { [UsedImplicitly] get; private set; } = null!;

    private void InitializeCommands()
    {
        AuthorizationViewLoadedCommand = new AsyncDelegateCommand(TestDatabaseConnectionAsync);
        ConnectionSetupCommand = new AsyncDelegateCommand(ShowConnectionSetupDialogAsync);
    }
    
    #endregion

    private async Task TestDatabaseConnectionAsync()
    {
        try
        {
            await _busyIndicatorManager.ExecuteWithBusyIndicatorAndOverlayAsync(async () =>
            {
                // Gets the connection string from local key service
                var connectionString = await _dbKeyService.LoadKeyAsync();
                
                // Testing this connection string
                await _dbConnectionService.TestConnectionAsync(connectionString);
                
                // Shows success snackbar
                NotificationManager.Show(Strings.SnackbarMessage_DbConnectionEstablished, NotificationType.Success);
                
                return Task.CompletedTask;
            }, _overlayManager, this, this, Strings.TestingDbConnection);

            IsConnectedToDatabase = true;
        }
        catch (Exception e)
        {
            IsConnectedToDatabase = false;
            await ShowConnectionFailedDialogAsync(e.Message); // Shows connection failed dialog if we have exception in test connection service method
        }
    }

    private async Task ShowConnectionSetupDialogAsync()
    {
        // Show dialog and await dialog result
        var dialogResult = await _overlayManager.ExecuteWithOverlayAsync(
            () => _dialogManager.ShowDialogAsync(DialogType.ConnectionSetup, this),
            this);
        
        // Returns if dialog canceled
        if(dialogResult.Result == ButtonResult.Cancel)
            return;
        
        // Test connection 
        await TestDatabaseConnectionAsync();
    }

    private async Task ShowConnectionFailedDialogAsync(string connectionExceptionMessage)
    {
        var dialogParameters = new DialogParameters
        {
            {ParameterKeys.ConnectionFailedDialogParameterExMessageKey, connectionExceptionMessage} // Put exception message in dialog parameters
        };
        
        // Show dialog and await dialog result
        var dialogResult = await _overlayManager.ExecuteWithOverlayAsync(
            () => _dialogManager.ShowDialogAsync(dialogType: DialogType.ConnectionFailed, dialogHost: this, parameters: dialogParameters),
            this);
        
        // Returns if dialog result does not have parameters (dialog canceled)
        if(dialogResult.Parameters.Count == 0)
            return;
        
        // Gets connection failed dialog result from parameters
        var connectionFailedDialogResult = dialogResult.Parameters
            .GetValue<ConnectionFailedDialogResult>(ParameterKeys.ConnectionFailedDialogResultKey);


       await ProcessConnectionFailedDialogResultAsync(connectionFailedDialogResult); // Process dialog result
    }

    private async Task ProcessConnectionFailedDialogResultAsync(ConnectionFailedDialogResult connectionFailedDialogResult)
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
                throw new ArgumentOutOfRangeException(nameof(connectionFailedDialogResult), connectionFailedDialogResult, null);
        } 
    }
}
    
    