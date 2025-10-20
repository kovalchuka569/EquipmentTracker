using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Core.Interfaces;
using JetBrains.Annotations;
using Notification.Wpf;
using Presentation.Interfaces;
using Presentation.Models;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Users;
using Presentation.ViewModels.DialogViewModels;
using Prism.Commands;
using Unity;

namespace Presentation.ViewModels;

public class UserManagerViewModel : InteractiveViewModelBase
{
    [Dependency]
    public required IAppLogger<UserManagerViewModel> Logger { get; init; } = null!;
    
    [Dependency]
    public required IUserManagerService UserManagerService { get; init; } = null!;
    
    [Dependency]
    public required ISfDataGridExportManager SfDataGridExportManager { get; init; } = null!;
    
    [Dependency]
    public required NotificationManager NotificationManager { get; init; } = null!;

    private ObservableCollection<DataGridUserViewModel> _users = [];
    
    public ObservableCollection<DataGridUserViewModel> Users
    {
        get => _users;
        set => SetProperty(ref _users, value);
    }
    
    [UsedImplicitly]
    public GridPrintParameters GridPrintParameters => SfDataGridExportManager.GridPrintParameters;
    
    public UserManagerViewModel()
    {
        InitializeCommands();
    }
    
    #region Commands management
    
    [UsedImplicitly]
    public DelegateCommand GridPrintCommand => SfDataGridExportManager.GridPrintCommand;
    public AsyncDelegateCommand UsersDataGridLoadedCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand UserRequestsCommand { [UsedImplicitly] get; private set; } = null!;
    public AsyncDelegateCommand RefreshDataGridCommand { [UsedImplicitly] get; private set; } = null!;

    private void InitializeCommands()
    {
        UserRequestsCommand = new AsyncDelegateCommand(OnUserRequestsCommandExecuted);
        UsersDataGridLoadedCommand = new AsyncDelegateCommand(OnDataGridLoadedCommandExecuted);
        RefreshDataGridCommand = new AsyncDelegateCommand(LoadUsersAsync);
    }
    
    #endregion

    private async Task OnDataGridLoadedCommandExecuted()
    {
        await InitializeAction(async () =>
        {
            await LoadUsersAsync();
        });
    }
    
    private async Task LoadUsersAsync()
    {
        Users.Clear();

        var userModels = await UserManagerService.GetActiveUsersAsync();

        foreach (var userModel in userModels.OrderBy(u => u.FirstName))
            Users.Add(new DataGridUserViewModel(userModel));
    }

    private async Task OnUserRequestsCommandExecuted()
    { 
        await DialogService 
            .Show<UserRequestsViewModel>()
            .In(this)
            .WithOverlay()
            .Await();
    }
}