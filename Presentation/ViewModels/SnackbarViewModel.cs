using System.Collections.ObjectModel;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Presentation.EventArgs;
using Presentation.Events;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.Common.Snackbar;
using Prism.Commands;
using Prism.Events;
using Unity;

namespace Presentation.ViewModels;

public class SnackbarViewModel : ViewModelBase
{
    #region Dependencies
    
    [Dependency] public required IEventAggregator EventAggregator = null!;
    
    #endregion
    
    #region Private fields
    
    private ObservableCollection<SnackViewModel> _activeSnacks = [];
    
    #endregion
    
    #region Properties

    public ObservableCollection<SnackViewModel> ActiveSnacks
    {
        get => _activeSnacks;
        set => SetProperty(ref _activeSnacks, value);
    }
    
    #endregion
    
    #region Constructor

    public SnackbarViewModel()
    {
        InitializeCommands();
    }
    
    #endregion
    
    #region Commands management
    
    public DelegateCommand<object> CloseSnackbarItemCommand { [UsedImplicitly] get; set; } = null!;
    public DelegateCommand SnackbarLoadedCommand { [UsedImplicitly] get; set; } = null!;

    private void InitializeCommands()
    {
        CloseSnackbarItemCommand = new DelegateCommand<object>(OnCloseSnackbarItemCommandExecuted);
        SnackbarLoadedCommand = new DelegateCommand(OnSnackbarLoadedCommandExecuted);
    }
    
    #endregion
    
    #region Commands executors

    private void OnSnackbarLoadedCommandExecuted()
    {
        EventAggregator.GetEvent<ShowSnackEvent>().Subscribe(ShowSnack);
    }
    
    private void OnCloseSnackbarItemCommandExecuted(object parameter)
    {
        var snack = (SnackViewModel) parameter;
        
        RemoveSnack(snack);
    }

    
    #endregion

    #region Public methods
    
    public void ShowSnack(ShowSnackEventArgs e)
    {
        var newSnack = new SnackViewModel().FromDomain(e.Snack);
        
        ActiveSnacks.Add(newSnack);
        StartAutoRemove(newSnack);
    }
    
    public async void RemoveSnack(SnackViewModel snack)
    {
        snack.Showed = false;
        
        // Delay for hiding animation (maybe refactor)
        await Task.Delay(300);
        
        ActiveSnacks.Remove(snack);
    }
    
    #endregion
    
    #region Private methods
    
    private async void StartAutoRemove(SnackViewModel snack)
    {
        if(snack.ShowTime <= 0)
            return;
        
        await Task.Delay(snack.ShowTime);
        
        if(!ActiveSnacks.Contains(snack))
            return;
        
        RemoveSnack(snack);
    }
    
    #endregion
}