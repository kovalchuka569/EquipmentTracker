using System.Threading.Tasks;
using JetBrains.Annotations;
using Presentation.ViewModels.Common;
using Presentation.ViewModels.DialogViewModels;
using Prism.Commands;

namespace Presentation.ViewModels;

public class SettingsViewModel : InteractiveViewModelBase
{
    #region Constructor
    
    public SettingsViewModel()
    {
        InitializeCommands();
    }
    
    #endregion

    #region Command management
    
    public AsyncDelegateCommand ShowRemoveMarkedItemsDialogCommand
    {
        [UsedImplicitly]
        get;
        private set;
    } = null!;
    
    private void InitializeCommands()
    {
        ShowRemoveMarkedItemsDialogCommand = new AsyncDelegateCommand(OnShowRemoveMarkedItemsDialog);
    }
    
    #endregion

    private async Task OnShowRemoveMarkedItemsDialog()
    {
        await DialogService 
            .Show<MarkedItemsCleanerViewModel>()
            .In(this)
            .WithOverlay()
            .Await();
    }
    
}