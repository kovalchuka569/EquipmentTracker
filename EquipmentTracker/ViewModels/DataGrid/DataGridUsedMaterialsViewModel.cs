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
    private string _consumablesTableName;
    
    private ObservableCollection<RepairConsumableItem> _usedMaterials = new();
    private RepairConsumableItem _selectedUsedMaterial;
    private bool _materialSelectorOpened;

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

    public bool AreUsedMaterialsEmpty => !UsedMaterials.Any();
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public DelegateCommand UserControlUnloadedCommand { get; }
    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand { get; }
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
        ShowMaterialSelector = new DelegateCommand(OnShowMaterialSelector);
        PopupConsumablesTreeLoadedCommand = new DelegateCommand(OnPopupConsumablesTreeLoaded);
        DeleteConsumableCommand = new DelegateCommand(OnDeleteConsumable);

        UsedMaterials.CollectionChanged += UsedMaterials_CollectionChanged;
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
            await _addRepairService.InsertUsedMaterialsAsync(UsedMaterials, repairId, _consumablesTableName);
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
            ConsumableTableName = args.ConsumableTableName
        };
        UsedMaterials.Add(newRepairConsumableItem);
        SelectedUsedMaterial = newRepairConsumableItem;
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
        {
            _scopedEventAggregator = scopedEventAggregator;
        }
        if (navigationContext.Parameters["ConsumablesTableName"] is string consumablesTableName)
        {
           _consumablesTableName = consumablesTableName;
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}