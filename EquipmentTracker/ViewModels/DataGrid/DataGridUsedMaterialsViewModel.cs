using System.Collections.ObjectModel;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Core.Events.DataGrid.Consumables;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Prism.Events;

namespace UI.ViewModels.DataGrid;

public class DataGridUsedMaterialsViewModel: BindableBase, INavigationAware
{
    private IRegionManager _regionManager;
    private readonly IEventAggregator _eventAggregator;
    
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
    public DelegateCommand ShowMaterialSelector  { get; set; }
    public DelegateCommand PopupConsumablesTreeLoadedCommand { get; set; }
    public DataGridUsedMaterialsViewModel(IEventAggregator eventAggregator)
    {
        _eventAggregator = eventAggregator;
        
        ShowMaterialSelector = new DelegateCommand(OnShowMaterialSelector);
        PopupConsumablesTreeLoadedCommand = new DelegateCommand(OnPopupConsumablesTreeLoaded);
        _eventAggregator.GetEvent<ConsumableSelectedEvent>().Subscribe(AddUsedMaterial);
        _eventAggregator.GetEvent<SaveUsedMaterialsEvent>().Subscribe(OnSaveMaterials);
    }

    // Send materials in service
    private void OnSaveMaterials()
    {
        
    }
    private void OnShowMaterialSelector()
    {
        MaterialSelectorOpened = true;
    }

    private void OnPopupConsumablesTreeLoaded()
    {
        var parameters = new NavigationParameters();
        parameters.Add("ScopedRegionManager", _regionManager);
        _regionManager.RequestNavigate("ConsumablesTreeSelectorRegion", "ConsumablesTreeSelectorView", parameters);
    }

    private void AddUsedMaterial(ConsumableItem consumableItem)
    {
        MaterialSelectorOpened = false;
        
        var newRepairConsumableItem = new RepairConsumableItem()
        {
            MaterialId = consumableItem.Id,
            Name = consumableItem.Name,
            Category = consumableItem.Category,
            Unit = consumableItem.Unit,
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
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext) { }
}