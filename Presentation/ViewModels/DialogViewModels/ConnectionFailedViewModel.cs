using System.Windows;
using Common.Enums;
using Common.Constants;
using JetBrains.Annotations;
using Presentation.ViewModels.DialogViewModels.Common;
using Prism.Commands;
using Prism.Dialogs;

namespace Presentation.ViewModels.DialogViewModels;

public class ConnectionFailedViewModel : DialogViewModelBase
{
    
    #region Private Fields
    
    private string _connectionErrorMessage = string.Empty;
    
    #endregion

    #region Properties
    
    public string ConnectionErrorMessage
    {
        get => _connectionErrorMessage;
        set => SetProperty(ref _connectionErrorMessage, value);
    }
    
    #endregion
    
    #region Constructor
    
    public ConnectionFailedViewModel()
    {
        InitializeCommands();
    }
    
    #endregion

    #region Commands management
    
    public DelegateCommand CancelDialogCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;

    public DelegateCommand TryAgainCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;

    public DelegateCommand ConnectionSetupCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;
    
    public DelegateCommand ExitCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;
    
    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnDialogClosed);
        TryAgainCommand = new DelegateCommand(OnTryAgainCommandExecuted);
        ConnectionSetupCommand = new DelegateCommand(OnConnectionSetupCommandExecuted);
        ExitCommand = new DelegateCommand(ExitCommandExecuted);
    }
    
    #endregion

    #region Private methods
    
    private void OnTryAgainCommandExecuted()
    {
        var result = new DialogResult
        {
            Parameters = new DialogParameters
            {
                { ParameterKeys.ConnectionFailedDialogResultKey, ConnectionFailedDialogResult.TryAgain }
            }
        };
        OnDialogClosed(result);
    }

    private void OnConnectionSetupCommandExecuted()
    {
        var result = new DialogResult
        {
            Parameters = new DialogParameters
            {
                { ParameterKeys.ConnectionFailedDialogResultKey, ConnectionFailedDialogResult.ConnectionSetup }
            }
        };
        OnDialogClosed(result);
    }
    
    private void ExitCommandExecuted()
    {
        Application.Current.Shutdown();
    }

    public override void OnDialogOpened(IDialogParameters parameters)
    {
        ConnectionErrorMessage = parameters.GetValue<string>(ParameterKeys.ConnectionFailedDialogParameterExMessageKey);
    }
    
    #endregion
}