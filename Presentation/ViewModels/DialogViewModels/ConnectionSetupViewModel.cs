using System.Threading.Tasks;
using Common.Logging;
using JetBrains.Annotations;
using Notification.Wpf;
using Prism.Commands;
using Resources.Localization;
using LocalDbConnectionService.Interfaces;
using Prism.Dialogs;

namespace Presentation.ViewModels.DialogViewModels;

public class ConnectionSetupViewModel : BaseDialogViewModel<ConnectionSetupViewModel>
{
    #region Constants
    
    private const string SaveKeyErrorLogMessage = "Error while saving key";

    #endregion

    #region Dependencies
    
    private readonly IDbKeyService _dbKeyService;

    #endregion

    #region Private fields

    private string _host = string.Empty;
    private string _port = string.Empty;
    private string _database = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;

    #endregion

    #region Properties

    public string Host
    {
        get => _host;
        set => SetProperty(ref _host, value);
    }

    public string Port
    {
        get => _port;
        set => SetProperty(ref _port, value);
    }

    public string Database
    {
        get => _database;
        set => SetProperty(ref _database, value);
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
    
    #region Constructor

    public ConnectionSetupViewModel(NotificationManager notificationManager,
        IAppLogger<ConnectionSetupViewModel> logger,
        IDbKeyService dbKeyService)
        : base(notificationManager, logger)
    {
        _dbKeyService = dbKeyService;

        InitializeCommands();
    }
    
    #endregion

    #region Commands management

    public DelegateCommand CancelDialogCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand SaveCommand { [UsedImplicitly] get; private set; } = null!;

    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnDialogClosed);
        SaveCommand = new AsyncDelegateCommand(OnSaveCommandExecutedAsync);
    }

    #endregion

    #region Private methods
    
    private async Task OnSaveCommandExecutedAsync()
    {
        await ExecuteWithErrorHandlingAsync(async () =>
        {
            var connectionString = $"Host={Host};Port={Port};Database={Database};Username={Username};Password={Password}";
            
            // Save connection string through the local db key service
            await _dbKeyService.SaveKeyInFileAsync(connectionString);

            // Show success snackbar
            NotificationManager.Show(Strings.SnackbarMessage_KeySaveSuccess, NotificationType.Success);
            
            // Close connection setup dialog
            var result = new DialogResult
            {
                Result = ButtonResult.OK
            };
            OnDialogClosed(result);
            
        }, Strings.SnackbarMessage_KeySaveFailed, 
            SaveKeyErrorLogMessage, 
            onFinally: ClearFields); 
    }

    private void ClearFields()
    {
        Host = string.Empty;
        Port = string.Empty;
        Database = string.Empty;
        Username = string.Empty;
        Password = string.Empty;
    }
    
    #endregion
}