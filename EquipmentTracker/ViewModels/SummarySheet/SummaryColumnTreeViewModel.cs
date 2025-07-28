using System.Collections.ObjectModel;
using Common.Logging;
using Core.Services.Summary;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Constants.Summary;
using EquipmentTracker.Events.Summary;
using Models.EquipmentTree;
using Models.Summary.ColumnTree;
using Syncfusion.UI.Xaml.TreeView;
using FolderItem = Models.Summary.ColumnTree.FolderItem;

namespace EquipmentTracker.ViewModels.SummarySheet;

public class SummaryColumnTreeViewModel : BindableBase, INavigationAware, IDestructible
{
    private bool _isInitialized;
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(_isInitialized) return;
        
        GetNavigationParameters(navigationContext.Parameters);
        
        await LoadItemsAsync();

        await GetSelectedColumnsIds();
        
        _isInitialized = true;
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) {}
    
    // DI
    private ISummaryService _summaryService;
    private IAppLogger<SummaryColumnTreeViewModel> _logger;

    // UI data
    private ObservableCollection<ISummaryFileSystemItem> _items = new();
    public ObservableCollection<ISummaryFileSystemItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private ObservableCollection<ISummaryFileSystemItem> _selectedItems = new();
    public ObservableCollection<ISummaryFileSystemItem> SelectedItems
    {
        get => _selectedItems;
        set => SetProperty(ref _selectedItems, value);
    }

    private bool _progressbarVisibility;
    public bool ProgressbarVisibility
    {
        get => _progressbarVisibility;
        set => SetProperty(ref _progressbarVisibility, value);
    }
    
    // Commands
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeExpandedCommand { get; }
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeCollapsedCommand { get; }
    public DelegateCommand<object> CheckedNodeCommand { get; }
    public DelegateCommand<object> UncheckedNodeCommand { get; }
    
    // Constructor
    public SummaryColumnTreeViewModel(ISummaryService summaryService, IAppLogger<SummaryColumnTreeViewModel> logger)
    {
        _summaryService = summaryService;
        _logger = logger;

        NodeExpandedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        NodeCollapsedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        CheckedNodeCommand = new DelegateCommand<object>(OnCheckedNode);
        UncheckedNodeCommand = new DelegateCommand<object>(OnUncheckedNode);
    }

    private List<int> _selectedColumnIds;
    private bool _selectionProcessing;
    
    private async Task GetSelectedColumnsIds()
    {
        if(_selectionProcessing)
            return;
        
        _selectionProcessing = true;
        try
        {
            _selectedColumnIds = await _summaryService.GetEquipmentSelectedColumnsIds(_summaryId, _summaryFormat);
            _summaryService.SelectInCollectionById(_items, _selectedColumnIds);
            _summaryService.UpdateParentSelectionStates(_items);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting selected columns ids");
            throw;
        }
        finally
        {
            _selectionProcessing = false;
        }
    }
    
    private async void OnCheckedNode(object parameter)
    {
        if(_selectionProcessing)
            return;
        if (parameter is not ISummaryFileSystemItem item)
            return;
        
        _selectionProcessing = true;
        try
        {
            _summaryService.SelectItemAndChildren(item);
            _summaryService.UpdateParentSelectionStates(_items);
            
            _selectedColumnIds = _summaryService.GetAllSelectedColumnIds(_items);
            
            // Update the selected summary columns ids in dbase
            await UpdateSelectedSummaryColumnsIds(CheckboxAction.Check);
            
            // Triggered datagrid for reload data
            _scopedEventAggregator?.GetEvent<RefreshSummaryTrigger>().Publish();
        }
        finally
        {
            _selectionProcessing = false;
        }
    }

    private async void OnUncheckedNode(object parameter)
    {
        if(_selectionProcessing)
            return;
        if (parameter is not ISummaryFileSystemItem item)
            return;
        
        _selectionProcessing = true;
        try
        {
            _summaryService.UnselectItemAndChildren(item);
            _summaryService.UpdateParentSelectionStates(_items);
            
            _selectedColumnIds = _summaryService.GetAllSelectedColumnIds(_items);

            // Update the selected summary columns ids in dbase
            await UpdateSelectedSummaryColumnsIds(CheckboxAction.Uncheck);
            
            // Triggered datagrid for reload data
           _scopedEventAggregator?.GetEvent<RefreshSummaryTrigger>().Publish();
        }
        finally
        {
            _selectionProcessing = false;
        }
    }
    
    private void OnNodeExpandedCollapsed(NodeExpandedCollapsedEventArgs e)
    {
        if (e.Node.Content is FolderItem folderItem)
        {
            if (e.Node.IsExpanded)
            {
                folderItem.ImageIcon = "Assets/opened_folder.png";
            }
            else
            {
                folderItem.ImageIcon = "Assets/folder.png";
            }
        }
    }
    

    private async Task LoadItemsAsync()
    {
        ProgressbarVisibility = true;
        try
        {
            Items = await _summaryService.GetHierarchicalItemsAsync(_summaryId, _summaryFormat);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error loading items");
            throw;
        }
        finally
        {
            await Task.Delay(300);
            ProgressbarVisibility = false;
        }
    }

    private async Task UpdateSelectedSummaryColumnsIds(CheckboxAction action)
    { 
        try
        {
            await _summaryService.UpdateSelectedSummaryColumnsIds(_summaryId, _selectedColumnIds, action, _summaryFormat);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating selected summary columns ids");
            throw;
        }
    }
    
    // Navigation context data
    private IRegionManager? _scopedRegionManager;
    private EventAggregator? _scopedEventAggregator;
    private int _summaryId;
    private SummaryFormat _summaryFormat;
    private void GetNavigationParameters(INavigationParameters parameters)
    {
        if (parameters[NavigationConstants.ScopedRegionManager] is IRegionManager scopedRegionManager)
        {
            _scopedRegionManager = scopedRegionManager;
        }
        if (parameters[NavigationConstants.ScopedEventAggregator] is EventAggregator scopedEventAggregator)
        {
            _scopedEventAggregator = scopedEventAggregator;
        }
        if (parameters[SummaryNavigationConstants.SummaryId] is int summaryId)
        {
            _summaryId = summaryId;
        }
        if (parameters[SummaryNavigationConstants.SummaryFormat] is SummaryFormat summaryFormat)
        {
            _summaryFormat = summaryFormat;
        }
    }

    public void Destroy()
    {
        _items = new ObservableCollection<ISummaryFileSystemItem>();
        _selectedItems = new ObservableCollection<ISummaryFileSystemItem>();
        _selectedColumnIds = new List<int>();
        _scopedRegionManager = null;
        _scopedEventAggregator = null;
    }
}