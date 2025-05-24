using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Common.Logging;
using Core.Events.DataGrid;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Core.Events.DataGrid.Consumables;
using Core.Services.RepairsDataGrid;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.ScrollAxis;
using Syncfusion.Windows.Controls.Grid;

namespace UI.ViewModels.DataGrid;

public class DataGridUsedMaterialsViewModel: BindableBase, INavigationAware
{
    private IRegionManager _regionManager;
    private EventAggregator _scopedEventAggregator;
    private readonly IAppLogger<DataGridUsedMaterialsViewModel> _logger;
    private readonly IAddRepairService _addRepairService;

    private SfDataGrid _sfDataGrid;
    private string _tableNameForSave;
    private int _workId;
    
    private ObservableCollection<RepairConsumableItem> _usedMaterials = new();
    private RepairConsumableItem _selectedUsedMaterial;
    private bool _materialSelectorOpened;
    private bool _removeMaterialContextMenuVisibility;

    public ObservableCollection<RepairConsumableItem> UsedMaterials
    {
        get => _usedMaterials;
        set => SetProperty(ref _usedMaterials, value);
    }

    public RepairConsumableItem SelectedUsedMaterial
    {
        get => _selectedUsedMaterial;
        set => SetProperty(ref _selectedUsedMaterial, value);
    }
    public bool MaterialSelectorOpened
    {
        get => _materialSelectorOpened;
        set => SetProperty(ref _materialSelectorOpened, value);
    }

    public bool RemoveMaterialContextMenuVisibility
    {
        get => _removeMaterialContextMenuVisibility;
        set => SetProperty(ref _removeMaterialContextMenuVisibility, value);
    }

    public bool AreUsedMaterialsEmpty => !UsedMaterials.Any();
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public DelegateCommand UserControlUnloadedCommand { get; }
    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand { get; }
    public DelegateCommand ContextMenuLoadedCommand { get; }
    public DelegateCommand ShowMaterialSelector  { get; }
    public DelegateCommand PopupConsumablesTreeLoadedCommand { get; }
    public DelegateCommand DeleteConsumableCommand { get; }
    public DataGridUsedMaterialsViewModel(IAppLogger<DataGridUsedMaterialsViewModel> logger, IAddRepairService addRepairService)
    {
        _logger = logger;
        _addRepairService = addRepairService;
        
        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
        UserControlUnloadedCommand = new DelegateCommand(OnUserControlUnloaded);
        SfDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnSfDataGridLoaded);
        ContextMenuLoadedCommand = new DelegateCommand(OnContextMenuLoaded);
        ShowMaterialSelector = new DelegateCommand(OnShowMaterialSelector);
        PopupConsumablesTreeLoadedCommand = new DelegateCommand(OnPopupConsumablesTreeLoaded);
        DeleteConsumableCommand = new DelegateCommand(OnDeleteConsumable);
        
    }

    private void OnContextMenuLoaded()
    {
        if (SelectedUsedMaterial is RepairConsumableItem item)
        {
            if (item.IsUserAdded)
            {
                RemoveMaterialContextMenuVisibility = true;
            }
            else
            {
                RemoveMaterialContextMenuVisibility = false;
            }
        }
    }

    private void UsedMaterials_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(AreUsedMaterialsEmpty));
    }

    private void OnSfDataGridLoaded(SfDataGrid dataGrid)
    {
        _sfDataGrid = dataGrid;
    }

    
    private SubscriptionToken _isAnyNullQuantityToken;
    private SubscriptionToken _isEmptyUsedMaterialsToken;
    private void OnUserControlLoaded()
    {
        _scopedEventAggregator.GetEvent<ConsumableSelectedEvent>().Subscribe(AddUsedMaterial);
        _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Subscribe(OnSaveMaterials);
        _isAnyNullQuantityToken = _scopedEventAggregator.GetEvent<IsAnyNullQuantityConsumables>().Subscribe(callback =>
        {
            bool result = UsedMaterials.Any(c => c.SpentMaterial == null);
            callback(result);
        });
        _isEmptyUsedMaterialsToken = _scopedEventAggregator.GetEvent<IsEmptyUsedMaterials>().Subscribe(callback =>
        {
            bool result = AreUsedMaterialsEmpty;
            callback(result);
        });
    }

    private void OnUserControlUnloaded()
    {
        _scopedEventAggregator.GetEvent<ConsumableSelectedEvent>().Unsubscribe(AddUsedMaterial);
        _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Unsubscribe(OnSaveMaterials);
        
        if (_isAnyNullQuantityToken != null)
        {
            _scopedEventAggregator.GetEvent<IsAnyNullQuantityConsumables>().Unsubscribe(_isAnyNullQuantityToken);
            _isAnyNullQuantityToken = null;
        }

        if (_isEmptyUsedMaterialsToken != null)
        {
            _scopedEventAggregator.GetEvent<IsEmptyUsedMaterials>().Unsubscribe(_isEmptyUsedMaterialsToken);
            _isEmptyUsedMaterialsToken = null;
        }
    }

    private async Task LoadUsedMaterials()
    {
        UsedMaterials = await _addRepairService.LoadUsedMaterialsAsync(_tableNameForSave, _workId);
        RaisePropertyChanged(nameof(AreUsedMaterialsEmpty));
    }

    private void OnDeleteConsumable()
    {
        if (SelectedUsedMaterial is RepairConsumableItem repairConsumableItem)
        {
            UsedMaterials.Remove(repairConsumableItem);
        }
    }

    // Save used materials
    private async void OnSaveMaterials(int repairId)
    {
        try
        {
            _logger.LogInformation("Request for transfer of used materials to the service");
            var userAddedMaterials = UsedMaterials.Where(x => x.IsUserAdded).ToList();
            if(userAddedMaterials.Count == 0) return;
            await _addRepairService.InsertUsedMaterialsAsync(userAddedMaterials, repairId, _tableNameForSave);
            _logger.LogInformation("The materials used were successfully transferred to the service");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while transfer of used materials to the service");
            throw;
        }
    }
    private void OnShowMaterialSelector()
    {
        MaterialSelectorOpened = true;
    }

    private void OnPopupConsumablesTreeLoaded()
    {
        var parameters = new NavigationParameters();
        parameters.Add("ScopedRegionManager", _regionManager);
        parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
        _regionManager.RequestNavigate("ConsumablesTreeSelectorRegion", "ConsumablesTreeSelectorView", parameters);
    }

    private void AddUsedMaterial(ConsumableSelectedEventArgs args)
    {
        MaterialSelectorOpened = false;
        
        var newRepairConsumableItem = new RepairConsumableItem
        {
            MaterialId = args.ConsumableItem.Id,
            Name = args.ConsumableItem.Name,
            Category = args.ConsumableItem.Category,
            Unit = args.ConsumableItem.Unit,
            ConsumableTableName = args.ConsumableTableName,
            IsUserAdded = true
        };
        UsedMaterials.Add(newRepairConsumableItem);
        SelectedUsedMaterial = newRepairConsumableItem;
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        UsedMaterials.CollectionChanged += UsedMaterials_CollectionChanged;
        
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
        {
            _scopedEventAggregator = scopedEventAggregator;
        }
        if (navigationContext.Parameters["RepairConsumablesTableName"] is string repairConsumablesTableName)
        {
            _tableNameForSave = repairConsumablesTableName;
        }
        if (navigationContext.Parameters["ServiceConsumablesTableName"] is string serviceConsumablesTableName)
        {
            _tableNameForSave = serviceConsumablesTableName;
        }
        if (navigationContext.Parameters["RepairId"] is int workId)
        {
            _workId = workId;
            await LoadUsedMaterials();
        }
        else if (navigationContext.Parameters["ServiceId"] is int serviceId)
        {
            _workId = serviceId;
            await LoadUsedMaterials();
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}