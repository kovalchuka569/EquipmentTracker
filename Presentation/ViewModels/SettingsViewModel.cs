using System;
using System.Threading.Tasks;
using Common.Logging;
using Core.Interfaces;
using JetBrains.Annotations;
using Notification.Wpf;
using Presentation.Enums;
using Presentation.Interfaces;
using Prism.Commands;

namespace Presentation.ViewModels;

public class SettingsViewModel : BaseViewModel<SettingsViewModel>, IDialogHost, IOverlayHost
{
    
    #region Dependencies

    private readonly IDialogManager _dialogManager;
    private readonly IOverlayManager _overlayManager;
    
    #endregion
    
    #region Private Fields
    
    private bool _isDialogOpen;
    private object? _dialogContent;
    private bool _isOverlayOpen;
    private object? _overlayContent;
    
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
    
    #endregion
    
    #region Constructor
    
    public SettingsViewModel(NotificationManager notificationManager, 
        IAppLogger<SettingsViewModel> logger,
        IDialogManager dialogManager,
        IOverlayManager overlayManager) 
        : base(notificationManager, logger)
    {
        _dialogManager = dialogManager;
        _overlayManager = overlayManager;
        
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
        _overlayManager.ShowOverlay(this);

       await _dialogManager.ShowDialogAsync(DialogType.RemoveMarkedItems, this);

        _overlayManager.HideOverlay(this);
        
    }
    
}