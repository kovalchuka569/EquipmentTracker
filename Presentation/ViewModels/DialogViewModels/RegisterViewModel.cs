using System;
using System.Threading.Tasks;
using Common.Enums;
using Common.Logging;
using Core.Interfaces;
using JetBrains.Annotations;
using LocalSecure.Managers;
using Models.Users;
using Presentation.Services.Interfaces;
using Presentation.ViewModels.DialogViewModels.Common;
using Prism.Commands;
using Resources.Localization;
using Unity;

namespace Presentation.ViewModels.DialogViewModels;

public class RegisterViewModel : DialogViewModelBase
{
    #region Constants
    
    private const string SendingErrorLogMessage = "Error while sending registration form.";
    
    #endregion
    
    #region Dependecies

    [Dependency] public required IAppLogger<RegisterViewModel> Logger = null!;
    
    [Dependency] public required IAuthService AuthService = null!;
    
    [Dependency] public required ISnackbarService SnackbarService = null!;
    
    #endregion
    
    #region Private fields
    
    private string _firstName = string.Empty;
    private string _lastName = string.Empty;
    private string _login = string.Empty;
    private string _password = string.Empty;
    
    #endregion

    #region Properties
    
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    public string Login
    {
        get => _login;
        set
        {
            if(SetProperty(ref _login, value))
                RemoveError(nameof(Login), ValidationUIMessages.LoginAlreadyTaken);
        }
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }
    
    #endregion
    
    #region Constructor

    public RegisterViewModel()
    {
        InitializeCommands();
    }
    
    #endregion
    
    #region Commands management
    
    public DelegateCommand CancelDialogCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand SendCommand { [UsedImplicitly] get; set; } = null!;

    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnDialogClosed);
        SendCommand = new AsyncDelegateCommand(OnSendCommandExecutedAsync);
    }
    
    #endregion

    #region Commands realization
    
    private async Task OnSendCommandExecutedAsync()
    {
        var loginExist = await AuthService.IsLoginExistAsync(_login);
        
        if (loginExist)
        {
            AddError(nameof(Login), ValidationUIMessages.LoginAlreadyTaken);
            return;
        }
        
        var newUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = _firstName,
            LastName = _lastName,
            Login = _login,
            PasswordHash = PasswordHasher.HashPassword(Password),
            Status = UserStatus.AwaitingConfirmation
        };

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            await AuthService.AddUserAsync(newUser);
            
            SnackbarService
                .Show()
                .WithMessage(Strings.RegsiterFormSended_SnackbarMessage)
                .OfType(SnackType.Success)
                .Now();

            OnDialogClosed();
        },
            onError: e =>
            {
                Logger.LogError(e, SendingErrorLogMessage);
                
                SnackbarService
                    .Show()
                    .WithMessage(Strings.ErrorWhileSendingRegisterForm_SnackbarMessage)
                    .OfType(SnackType.Warning)
                    .Now();
            });
    }
    
    #endregion
}