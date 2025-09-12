using System;
using System.Threading.Tasks;
using Common.Enums;
using Common.Logging;
using JetBrains.Annotations;
using Notification.Wpf;
using Prism.Commands;

namespace Presentation.ViewModels;

public class MarkedItemsCleanerViewModel : BaseDialogViewModel<MarkedItemsCleanerViewModel>
{
    #region Private fields
    
    private MarkedItemsRemovingType _markedItemsRemovingType;
    private MarkedItemsRemovingStep _markedItemsRemovingStep;
    
    #endregion
    
    #region Public fields

    public MarkedItemsRemovingType MarkedItemsRemovingType
    {
        get => _markedItemsRemovingType;
        set => SetProperty(ref _markedItemsRemovingType, value);
    }

    public MarkedItemsRemovingStep MarkedItemsRemovingStep
    {
        get => _markedItemsRemovingStep;
        set => SetProperty(ref _markedItemsRemovingStep, value);
    }
    
    #endregion

    #region Constructor
    
    public MarkedItemsCleanerViewModel(NotificationManager notificationManager,
        IAppLogger<MarkedItemsCleanerViewModel> logger) : base(notificationManager, logger)
    {
        InitializeCommands();
    }
    
    #endregion
    
    public DelegateCommand CloseDialogCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand OkButtonClickedCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<object> RemovingTypeChangedCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;
    

    private void InitializeCommands()
    {
        CloseDialogCommand = new DelegateCommand(OnDialogClosed);
        OkButtonClickedCommand = new DelegateCommand(OnOkButtonClicked);
        RemovingTypeChangedCommand = new DelegateCommand<object>(OnRemovingTypeChanged);
    }

    private async void OnOkButtonClicked()
    {
        switch (MarkedItemsRemovingStep)
        {
            case MarkedItemsRemovingStep.RemovingTypeSelection:
                switch (MarkedItemsRemovingType)
                {
                    case MarkedItemsRemovingType.Auto:
                        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingProcess;
                        await AutoRemovingProcess();
                        break;
                    case MarkedItemsRemovingType.Selection:
                        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingItemsSelection;
                        break;
                }
                break;
            case MarkedItemsRemovingStep.RemovingItemsSelection:
                MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingProcess;
                await RemovingProcess();
                break;
        }
    }

    private void OnRemovingTypeChanged(object parameter)
    {
        if (parameter is not MarkedItemsRemovingType removingType)
            return;
        
        MarkedItemsRemovingType = removingType;
    }

    private async Task AutoRemovingProcess()
    {
        Console.WriteLine(@"Start auto Removing Process");
        await Task.Delay(5000);
        Console.WriteLine(@"Auto Removing Finished");
        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingFinish;
    }

    private async Task RemovingProcess()
    {
        Console.WriteLine(@"Start selected removing Process");
        await Task.Delay(5000);
        Console.WriteLine(@"Start Removing Finished");
        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingFinish;
    }
}