using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Common.Enums;
using Common.Logging;
using JetBrains.Annotations;
using Notification.Wpf;
using Presentation.ViewModels.Common.MarkedItemsCleaner;
using Prism.Commands;

namespace Presentation.ViewModels.DialogViewModels;

public class MarkedItemsCleanerViewModel : BaseDialogViewModel<MarkedItemsCleanerViewModel>
{
    #region Private fields
    
    private MarkedItemsRemovingType _markedItemsRemovingType;
    private MarkedItemsRemovingStep _markedItemsRemovingStep;
    private ObservableCollection<BaseMarkedForDeleteItemViewModel> _markedForDeleteItems = [];
    private ObservableCollection<object> _checkedForDeleteItems = [];
    
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

    public ObservableCollection<BaseMarkedForDeleteItemViewModel> MarkedForDeleteItems
    {
        get => _markedForDeleteItems;
        set => SetProperty(ref _markedForDeleteItems, value);
    }

    public ObservableCollection<object> CheckedForDeleteItems
    {
        get => _checkedForDeleteItems;
        set => SetProperty(ref _checkedForDeleteItems, value);
    }
    
    #endregion

    #region Constructor
    
    public MarkedItemsCleanerViewModel(NotificationManager notificationManager,
        IAppLogger<MarkedItemsCleanerViewModel> logger) : base(notificationManager, logger)
    {
        InitializeCommands();

        CheckedForDeleteItems.CollectionChanged += OnCheckedItemsChanged;
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

    public DelegateCommand SelectedItemsTreeLoadedCommand
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
        SelectedItemsTreeLoadedCommand = new DelegateCommand(OnSelectedItemsTreeLoaded);
    }
    
    private readonly Random _random = new Random();

    private void OnSelectedItemsTreeLoaded()
    {
        var rootItemsCount = _random.Next(3, 8);
        for (int i = 0; i < rootItemsCount; i++)
        {
            var rootItem = CreateRandomItem(null);
            GenerateChildren(rootItem, 3); 
            MarkedForDeleteItems.Add(rootItem);
        }
    }

    private void OnCheckedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Console.WriteLine(CheckedForDeleteItems.Count);
    }
    
    private BaseMarkedForDeleteItemViewModel CreateRandomItem(Guid? parentId)
    {
        // Случайный выбор типа элемента
        var types = new[]
        {
            MarkedForDeleteItemType.Folder,
            MarkedForDeleteItemType.EquipmentSheet,
            MarkedForDeleteItemType.EquipmentSheetColumn,
            MarkedForDeleteItemType.EquipmentSheetRow,
            MarkedForDeleteItemType.PivotSheet
        };
    
        var randomType = types[_random.Next(types.Length)];
    
        BaseMarkedForDeleteItemViewModel item = randomType switch
        {
            MarkedForDeleteItemType.Folder => new MarkedForDeleteFolderViewModel(),
            MarkedForDeleteItemType.EquipmentSheet => new MarkedForDeleteEquipmentSheetViewModel(),
            MarkedForDeleteItemType.EquipmentSheetColumn => new MarkedForDeleteEquipmentSheetColumnViewModel(),
            MarkedForDeleteItemType.EquipmentSheetRow => new MarkedForDeleteEquipmentSheetRowViewModel(),
            MarkedForDeleteItemType.PivotSheet => new MarkedForDeletePivotSheetViewModel(),
            _ => new MarkedForDeleteFolderViewModel()
        };
    
        item.Id = Guid.NewGuid();
        item.ParentId = parentId;
        item.SetTitle($"{randomType} {_random.Next(1000)}");
        item.IsSelectedForDelete = _random.Next(2) == 1; // Случайный выбор
    
        return item;
    }

    private void GenerateChildren(BaseMarkedForDeleteItemViewModel parent, int maxDepth)
    {
        if (maxDepth <= 0) return;
    
        var childrenCount = _random.Next(0, 4); // 0-3 потомка
        for (int i = 0; i < childrenCount; i++)
        {
            var child = CreateRandomItem(parent.Id);
            GenerateChildren(child, maxDepth - 1);
            parent.AddChild(child);
        }
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
        await Task.Delay(5000);
        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingFinish;
    }

    private async Task RemovingProcess()
    {
        await Task.Delay(5000);
        MarkedItemsRemovingStep = MarkedItemsRemovingStep.RemovingFinish;
    }

    public override void OnDialogClosed()
    {
        Console.WriteLine(CheckedForDeleteItems.Count);
        base.OnDialogClosed();
    }
}