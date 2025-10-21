using System.Threading.Tasks;
using Common.Enums;
using Common.Logging;
using JetBrains.Annotations;
using Prism.Commands;
using Resources.Localization;
using LocalSecure.Interfaces;
using Presentation.Services.Interfaces;
using Presentation.ViewModels.DialogViewModels.Common;
using Prism.Dialogs;
using Unity;

namespace Presentation.ViewModels.DialogViewModels;

public class ConnectionSetupViewModel : DialogViewModelBase
{
    #region Constants
    
    private const string SaveKeyErrorLogMessage = "Error while saving key";

    #endregion

    #region Dependencies
    
    [Dependency] public required IAppLogger<ConnectionSetupViewModel> Logger = null!;
    
    [Dependency] public required IDbKeyService DbKeyService = null!;
    
    [Dependency] public required ISnackbarService SnackbarService = null!;

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

    public ConnectionSetupViewModel()
    {
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
                // Save connection string through the local db key service
                await DbKeyService.SaveDbConnectionStringInFileAsync(_host, _port, _database, _username, _password);

                // Show success snackbar
                SnackbarService
                    .Show()
                    .WithMessage(Strings.SnackbarMessage_KeySaveSuccess)
                    .OfType(SnackType.Success)
                    .Now();

                // Close connection setup dialog
                var result = new DialogResult
                {
                    Result = ButtonResult.OK
                };
                OnDialogClosed(result);
            },
            onFinally: ClearFields,
            onError: e =>
            {
                Logger.LogError(e, SaveKeyErrorLogMessage);
                
                SnackbarService
                    .Show()
                    .WithMessage(Strings.SnackbarMessage_KeySaveFailed)
                    .OfType(SnackType.Warning)
                    .Now();
            });
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