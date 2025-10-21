using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Enums;
using Common.Logging;
using Core.Interfaces;
using JetBrains.Annotations;
using Presentation.Services.Interfaces;
using Presentation.ViewModels.Common.Users;
using Presentation.ViewModels.DialogViewModels.Common;
using Prism.Commands;
using Resources.Localization;
using Unity;

namespace Presentation.ViewModels.DialogViewModels;

public class UserRequestsViewModel : DialogViewModelBase
{
    
    #region Constants
    
    private const string ApproveErrorLogMessage = "Error while approving user.";
    private const string RejectErrorLogMessage = "Error while rejecting user.";
    
    #endregion

    #region Dependecies
    
    [Dependency] public required IAppLogger<UserRequestsViewModel> Logger = null!;

    [Dependency] public required IUserManagerService UserManagerService = null!;
    
    [Dependency] public required ISnackbarService SnackbarService = null!;
    
    #endregion
    
    private ObservableCollection<QueryUserViewModel> _usersQuery = [];

    public ObservableCollection<QueryUserViewModel> UsersQuery
    {
        get => _usersQuery;
        set => SetProperty(ref _usersQuery, value);
    }
    
    public bool HasUsersInQuery => UsersQuery.Any();
    
    public UserRequestsViewModel()
    {
        UsersQuery.CollectionChanged += (_, _) => RaisePropertyChanged(nameof(HasUsersInQuery));
        
        InitializeCommands();
    }

    public DelegateCommand CancelDialogCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand UsersQueryListLoadedCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand<object> ApproveUserCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand<object> RejectUserCommand { [UsedImplicitly] get; private set; } = null!;

    private void InitializeCommands()
    {
        CancelDialogCommand = new DelegateCommand(OnDialogClosed);
        UsersQueryListLoadedCommand = new AsyncDelegateCommand(OnUsersQueryListLoadedCommandExecuted);
        ApproveUserCommand = new AsyncDelegateCommand<object>(OnApproveUserCommandExecuted);
        RejectUserCommand = new AsyncDelegateCommand<object>(OnRejectUserCommandExecuted);
    }

    private async Task OnUsersQueryListLoadedCommandExecuted()
    {
        UsersQuery.Clear();

        var queryUserModels = await UserManagerService.GetAwaitConfirmationUsersAsync();

        foreach (var queryUserModel in queryUserModels.OrderByDescending(q => q.AccessRequestedAt))
            UsersQuery.Add(new QueryUserViewModel(queryUserModel));
    }

    private async Task OnApproveUserCommandExecuted(object arg)
    {
        if(arg is not QueryUserViewModel approvedUser)
            return;

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            await UserManagerService.ApproveQueryUserAsync(approvedUser.Id);
            UsersQuery.Remove(approvedUser);
            
        }, 
            onError: e =>
            {
                Logger.LogError(e, ApproveErrorLogMessage);
                
                SnackbarService
                    .Show()
                    .WithMessage(SnackbarMessages.ApproveQueryUser_ErrorMessage)
                    .OfType(SnackType.Warning)
                    .Now();
            });
    }
    
    private async Task OnRejectUserCommandExecuted(object arg)
    {
        if(arg is not QueryUserViewModel rejectedUser)
            return;

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            await UserManagerService.RejectQueryUserAsync(rejectedUser.Id);
            UsersQuery.Remove(rejectedUser);
            
        }, 
        onError: e =>
        {
            Logger.LogError(e, RejectErrorLogMessage);
            
            SnackbarService
                .Show()
                .WithMessage(SnackbarMessages.RejectQueryUser_ErrorMessage)
                .OfType(SnackType.Warning)
                .Now();
        });
    }
}