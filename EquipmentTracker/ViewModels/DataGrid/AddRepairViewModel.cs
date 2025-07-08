using System.Collections.ObjectModel;
using System.Windows.Forms;
using Core.Events.DataGrid;
using Prism.Mvvm;
using Prism.Commands;
using Core.Events.DataGrid.Consumables;
using Core.Events.TabControl;
using Core.Services.RepairsDataGrid;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Notification.Wpf;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;

namespace UI.ViewModels.DataGrid;

public class AddRepairViewModel : BindableBase, INavigationAware
{
    private IRegionManager _regionManager;
    private NotificationManager _notificationManager;
    private readonly IAddRepairService _service;
    private EventAggregator _scopedEventAggregator;
    private IEventAggregator _globalEventAggregator;
    
    private string _equipmentTableName;
    private int _repairId;
    
    
    private bool _isEditMode;
    private bool _repairEquipmentTextBlockVisibility;
    private bool _equipmentSelectorVisibility;
    
    private ObservableCollection<EquipmentItem> _equipments = new();
    private EquipmentItem _selectedEquipment;
    private List<RepairStatusItem> _repairStatuses = new();
    private RepairStatusItem _selectedRepairStatus;

    #region Fields

    private DateTime? _dateTimeStartRepair;
    private DateTime? _dateTimeEndRepair;
    private TimeSpan? _timeSpentOnRepair;
    private string? _breakDescription;
    private string _repairObjectDisplay;
    private string _mainButtonContent;
    

    #endregion

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }
    
    public ObservableCollection<EquipmentItem> Equipments
    {
        get => _equipments;
        set => SetProperty(ref _equipments, value);
    }
    public EquipmentItem SelectedEquipment
    {
        get => _selectedEquipment;
        set => SetProperty(ref _selectedEquipment, value);
    }
    public List<RepairStatusItem> RepairStatuses
    {
        get => _repairStatuses;
        set => SetProperty(ref _repairStatuses, value);
    }
    public RepairStatusItem SelectedRepairStatus
    {
        get => _selectedRepairStatus;
        set => SetProperty(ref _selectedRepairStatus, value);
    }

    public DateTime? DateTimeStartRepair
    {
        get => _dateTimeStartRepair;
        set => SetProperty(ref _dateTimeStartRepair, value);
    }

    public DateTime? DateTimeEndRepair
    {
        get => _dateTimeEndRepair;
        set
        {
            if (SetProperty(ref _dateTimeEndRepair, value))
            {
                OnDateTimeEndRepairChanged();
            }
        }
    }

    public TimeSpan? TimeSpentOnRepair
    {
        get => _timeSpentOnRepair;
        set => SetProperty(ref _timeSpentOnRepair, value);
    }

    public string? BreakDescription
    {
        get => _breakDescription;
        set => SetProperty(ref _breakDescription, value);
    }

    public string RepairObjectDisplay
    {
        get => _repairObjectDisplay;
        set => SetProperty(ref _repairObjectDisplay, value);
    }

    public bool RepairEquipmentTextBlockVisibility
    {
        get => _repairEquipmentTextBlockVisibility;
        set => SetProperty(ref _repairEquipmentTextBlockVisibility, value);
    }

    public bool EquipmentSelectorVisibility
    {
        get => _equipmentSelectorVisibility;
        set => SetProperty(ref _equipmentSelectorVisibility, value);
    }

    public string MainButtonContent
    {
        get => _mainButtonContent;
        set => SetProperty(ref _mainButtonContent, value);
    }
    
    
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public DelegateCommand SaveRepairCommand { get; }
    
    public AddRepairViewModel(IAddRepairService service,
        NotificationManager notificationManager,
        IEventAggregator globalEventAggregator)
    {
        _service = service;
        _notificationManager = notificationManager;
        _globalEventAggregator = globalEventAggregator;
        
        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
        SaveRepairCommand = new DelegateCommand(OnSaveRepair);
        
        DateTimeStartRepair = DateTime.Now;
        DateTimeEndRepair = null;
        TimeSpentOnRepair = null;
    }
    
    private void OnUserControlLoaded()
    {
        var parameters = new NavigationParameters();
        parameters.Add("ScopedRegionManager", _regionManager);
        parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
        parameters.Add("RepairConsumablesTableName", $"{_equipmentTableName} РВМ");
        parameters.Add("RepairId", _repairId);
        _regionManager.RequestNavigate("DataGridUsedMaterialsRegion", "DataGridUsedMaterialsView", parameters);
    }
    

    private async void OnSaveRepair()
    {
        if (DataValidate()) return;
        
        var newRepairData = new RepairData
        {
            EquipmentId = SelectedEquipment.EquipmentId,
            StartRepair = DateTimeStartRepair,
            EndRepair = DateTimeEndRepair,
            TimeSpentOnRepair = TimeSpentOnRepair,
            BreakDescription = BreakDescription,
            Worker = 1,
            RepairStatus = SelectedRepairStatus.StatusName
        };
        string repairTableName = _equipmentTableName + " Р";
        
        // Check have empty materials collection
        bool isEmptyMaterial = false;
        _scopedEventAggregator.GetEvent<IsEmptyUsedMaterials>().Publish(result => isEmptyMaterial = result);
        
        if (!IsEditMode)
        {
            try
            {
                // Save repair
                int repairId = await _service.SaveRepairAsync(newRepairData, repairTableName);
                // If dont have empty - save used materials and write-off 
                if (!isEmptyMaterial)
                {
                    _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Publish(repairId);
                }
            
                // Close tab
                _globalEventAggregator.GetEvent<CloseActiveTabEvent>().Publish();
                _notificationManager.Show("Успішно збережено!", NotificationType.Success);
            }
            catch (Exception e)
            {
                _notificationManager.Show($"Помилка додавання ремонту: {e.Message}", NotificationType.Error);
                throw;
            }
        }
        // If edit mode
        else if (IsEditMode)
        {
            try
            {
                await _service.UpdateRepairAsync(newRepairData, repairTableName, _repairId);
                // Save (in DataGiridUsedMaterials updating)
                if (!isEmptyMaterial)
                {
                    _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Publish(_repairId);
                }
                
                // Close tab
                _globalEventAggregator.GetEvent<CloseActiveTabEvent>().Publish();
                _notificationManager.Show("Успішно збережено!", NotificationType.Success);
            }
            catch (Exception e)
            {
                _notificationManager.Show($"Помилка збереження ремонту: {e.Message}", NotificationType.Error);
                throw;
            }
        }
        
    }

    private bool DataValidate()
    {
        bool quantityIsNull = false;
        _scopedEventAggregator.GetEvent<IsAnyNullQuantityConsumables>().Publish(result => quantityIsNull = result);
        
        if (SelectedEquipment is null)
        {
            _notificationManager.Show("Об'єкт ремонту не може бути порожнім", NotificationType.Warning);
            return true;
        }
        if (SelectedRepairStatus is null)
        {
            _notificationManager.Show("Статус ремонту не може бути порожнім", NotificationType.Warning);
            return true;
        }
        if (!string.IsNullOrWhiteSpace(BreakDescription) && BreakDescription.Length > 2000)
        {
            _notificationManager.Show("Масимальна довжина опису поломки - 2000 символів", NotificationType.Warning);
            return true;
        }
        if (quantityIsNull)
        {
            _notificationManager.Show("Встановіть кількість витраченого матеріалу", NotificationType.Warning);
            return true;
        }
        if (SelectedRepairStatus.StatusName == "Заплановано" && !DateTimeStartRepair.HasValue)
        {
            _notificationManager.Show("Для статусу \"Заплановано\" потрібно встановити дату початку ремонту", NotificationType.Warning);
            return true;
        }
        return false;
    }
    
    private void OnDateTimeEndRepairChanged()
    {
        if (DateTimeEndRepair.HasValue && DateTimeStartRepair.HasValue)
        {
            if (DateTimeEndRepair >= DateTimeStartRepair)
            {
                TimeSpentOnRepair = DateTimeEndRepair - DateTimeStartRepair;
            }
            else
            {
                TimeSpentOnRepair = null;
            }
        }
        else
        {
            TimeSpentOnRepair = null;
        }
    }

    private async Task LoadEquipmentsAsync()
    {
        Equipments = await _service.GetEquipmentItemsAsync(_equipmentTableName);
    }

    private void LoadRepairStatuses()
    {
        RepairStatuses.Clear();
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Заплановано"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Очікує підтвердження"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Очікує матеріали"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "В процессі"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Призупинений"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Виконано"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Скасовано"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Діагностика"});
        RepairStatuses.Add(new RepairStatusItem {StatusName = "Передано підряднику"});
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
        
        // Set table name value before loading equipments for selector
        _equipmentTableName = navigationContext.Parameters["EquipmentTableName"].ToString();
        
        LoadRepairStatuses();

        // If have parameter RepairItem - its EditMode! Load repair data in model
        if (navigationContext.Parameters["RepairItem"] is RepairItem repairItem)
        {
            // Make edit mode true
            IsEditMode = true;
            
            // Load selected equipment
            SelectedEquipment = new EquipmentItem
            {
                EquipmentId = repairItem.EquipmentId,
                EquipmentInventoryNumber = repairItem.EquipmentInventoryNumber,
                EquipmentBrand = repairItem.EquipmentBrand,
                EquipmentModel = repairItem.EquipmentModel,
            };
            
            // Load other data
            DateTimeStartRepair = repairItem.StartDate;
            DateTimeEndRepair = repairItem.EndDate;
            TimeSpentOnRepair = repairItem.Duration;
            BreakDescription = repairItem.BreakDescription;
            
            // Select repair status
            SelectedRepairStatus = RepairStatuses.First(s => s.StatusName == repairItem.Status);

            _repairId = repairItem.Id;

            RepairEquipmentTextBlockVisibility = true;
            RepairObjectDisplay = $"{SelectedEquipment.EquipmentInventoryNumber} | {SelectedEquipment.EquipmentBrand} | {SelectedEquipment.EquipmentModel}";

            MainButtonContent = "Зберегти";
        }
        

        // Load equipment selector items only in creating mode
        if (!IsEditMode)
        {
            LoadEquipmentsAsync();
            EquipmentSelectorVisibility = true;
            MainButtonContent = "Створити";
        }
        
    }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}