using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Core.Events.DataGrid;
using Core.Events.DataGrid.Consumables;
using Core.Events.TabControl;
using Core.Services.ServicesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;
using Models.RepairsDataGrid.ServicesDataGrid;
using Notification.Wpf;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Syncfusion.UI.Xaml.Grid;

namespace EquipmentTracker.ViewModels.DataGrid.Services;

public class AddServiceViewModel : BindableBase, INavigationAware
{
    private IRegionManager _scopedRegionManager;
    private EventAggregator _scopedEventAggregator;
    private readonly IRegionManager _globalRegionManager;
    private readonly IEventAggregator _globalEventAggregator;
    private readonly NotificationManager _globalNotificationManager;
    private readonly IAddServiceService _service;

    private SfMultiColumnDropDownControl _equipmentSelector;
    
    
    private string _equipmentTableName;
    private int _serviceId;

    private bool _isEditMode;
    private bool _serviceEquipmentTextBlockVisibility;
    private bool _equipmentSelectorVisibility;
    private string _serviceObjectDisplay;
    private string _serviceDescription;
    private string _mainButtonContent;
    private DateTime? _dateTimeStartService;
    private DateTime? _dateTimeEndService;
    private TimeSpan? _timeSpentOnService;
    
    private ObservableCollection<EquipmentItem> _equipments = new();
    private EquipmentItem _selectedEquipment;
    private List<ServiceStatusItem> _serviceStatuses = new();
    private ServiceStatusItem _selectedServiceStatus;
    private List<ServiceTypeItem> _serviceTypes = new();
    private ServiceTypeItem _selectedServiceType;
    
    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public string ServiceDescription
    {
        get => _serviceDescription;
        set => SetProperty(ref _serviceDescription, value);
    }

    public string MainButtonContent
    {
        get => _mainButtonContent;
        set => SetProperty(ref _mainButtonContent, value);
    }

    public bool ServiceEquipmentTextBlockVisibility
    {
        get => _serviceEquipmentTextBlockVisibility;
        set => SetProperty(ref _serviceEquipmentTextBlockVisibility, value);
    }

    public bool EquipmentSelectorVisibility
    {
        get => _equipmentSelectorVisibility;
        set => SetProperty(ref _equipmentSelectorVisibility, value);
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

    public List<ServiceStatusItem> ServiceStatuses
    {
        get => _serviceStatuses;
        set => SetProperty(ref _serviceStatuses, value);
    }

    public ServiceStatusItem SelectedServiceStatus
    {
        get => _selectedServiceStatus;
        set => SetProperty(ref _selectedServiceStatus, value);
    }

    public List<ServiceTypeItem> ServiceTypes
    {
        get => _serviceTypes;
        set => SetProperty(ref _serviceTypes, value);
    }

    public ServiceTypeItem SelectedServiceType
    {
        get => _selectedServiceType;
        set => SetProperty(ref _selectedServiceType, value);
    }

    public string ServiceObjectDisplay
    {
        get => _serviceObjectDisplay;
        set => SetProperty(ref _serviceObjectDisplay, value);
    }
    public DateTime? DateTimeStartService
    {
        get => _dateTimeStartService;
        set => SetProperty(ref _dateTimeStartService, value);
    }
    public DateTime? DateTimeEndService
    {
        get => _dateTimeEndService;
        set
        {
            if (SetProperty(ref _dateTimeEndService, value))
            {
                OnDateTimeEndRepairChanged();
            }
        }
    }
    public TimeSpan? TimeSpentOnService
    {
        get => _timeSpentOnService;
        set => SetProperty(ref _timeSpentOnService, value);
    }
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public DelegateCommand SaveServiceCommand { get; }
    
    public AddServiceViewModel(IRegionManager globalRegionManager, 
        IEventAggregator globalEventAggregator, 
        NotificationManager globalNotificationManager,
        IAddServiceService service)
    {
        _globalRegionManager = globalRegionManager;
        _globalEventAggregator = globalEventAggregator;
        _globalNotificationManager = globalNotificationManager;
        _service = service;

        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
        SaveServiceCommand = new DelegateCommand(OnSaveService);
        
        DateTimeStartService = DateTime.Now;
        DateTimeEndService = null;
        TimeSpentOnService = null;
    }

    private async void OnSaveService()
    {
        if(DataValidate()) return;

        var newServiceData = new ServiceData
        {
            EquipmentId = SelectedEquipment.EquipmentId,
            StartService = DateTimeStartService,
            EndService = DateTimeEndService,
            TimeSpentOnService = TimeSpentOnService,
            ServiceDescription = ServiceDescription,
            ServiceType = SelectedServiceType.TypeName,
            ServiceStatus = SelectedServiceStatus.StatusName,
            Worker = 1
        };
        string serviceTableName = _equipmentTableName + " О";
        
        // Check have empty materials collection
        bool isEmptyMaterial = false;
        _scopedEventAggregator.GetEvent<IsEmptyUsedMaterials>().Publish(result => isEmptyMaterial = result);

        if (!IsEditMode)
        {
            try
            {
                // Save repair
                int serviceId = await _service.SaveRepairAsync(newServiceData, serviceTableName);
                
                // If dont have empty - save used materials and write-off 
                if (!isEmptyMaterial)
                {
                    _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Publish(serviceId);
                }
            
                // Close tab
                _globalEventAggregator.GetEvent<CloseActiveTabEvent>().Publish();
                _globalNotificationManager.Show("Успішно збережено!", NotificationType.Success);
            }
            catch (Exception e)
            {
                _globalNotificationManager.Show($"Помилка додавання обслуговування: {e.Message}", NotificationType.Error);
                throw;
            }
        }

        else if (IsEditMode)
        {
            try
            {
                await _service.UpdateServiceAsync(newServiceData, serviceTableName, _serviceId);
                if (!isEmptyMaterial)
                {
                    _scopedEventAggregator.GetEvent<SaveUsedMaterialsEvent>().Publish(_serviceId);
                }
                
                _globalEventAggregator.GetEvent<CloseActiveTabEvent>().Publish();
                _globalNotificationManager.Show("Успішно збережено!", NotificationType.Success);
            }
            catch (Exception e)
            {
                _globalNotificationManager.Show($"Помилка збереження ремонту: {e.Message}", NotificationType.Error);
                throw;
            }
        }
    }
    
    private void OnDateTimeEndRepairChanged()
    {
        if (DateTimeStartService.HasValue && DateTimeStartService.HasValue)
        {
            if (DateTimeEndService >= DateTimeStartService)
            {
                TimeSpentOnService = DateTimeEndService - DateTimeStartService;
            }
            else
            {
                TimeSpentOnService = null;
            }
        }
        else
        {
            TimeSpentOnService = null;
        }
    }
    
    private async Task LoadEquipmentsAsync()
    {
        Equipments = await _service.GetEquipmentItemsAsync(_equipmentTableName);
    }
    
    private void OnUserControlLoaded()
    {
        var parameters = new NavigationParameters();
        parameters.Add("ScopedRegionManager", _scopedRegionManager);
        parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
        parameters.Add("ServiceConsumablesTableName", $"{_equipmentTableName} ОВМ");
        parameters.Add("ServiceId", _serviceId);
        _scopedRegionManager.RequestNavigate("DataGridUsedMaterialsRegion", "DataGridUsedMaterialsView", parameters);
    }
    
    private bool DataValidate()
    {
        bool quantityIsNull = false;
        _scopedEventAggregator.GetEvent<IsAnyNullQuantityConsumables>().Publish(result => quantityIsNull = result);
        
        if (SelectedEquipment is null)
        {
            _globalNotificationManager.Show("Об'єкт обслуговування не може бути порожнім", NotificationType.Warning);
            return true;
        }
        if (SelectedServiceType is null)
        {
            _globalNotificationManager.Show("Тип обслуговування не може бути порожнім", NotificationType.Warning);
            return true;
        }
        if (SelectedServiceStatus is null)
        {
            _globalNotificationManager.Show("Статус обслуговування не може бути порожнім", NotificationType.Warning);
            return true;
        }
        if (!string.IsNullOrWhiteSpace(ServiceDescription) && ServiceDescription.Length > 2000)
        {
            _globalNotificationManager.Show("Масимальна довжина опису осблуговування - 2000 символів", NotificationType.Warning);
            return true;
        }
        if (quantityIsNull)
        {
            _globalNotificationManager.Show("Встановіть кількість витраченого матеріалу", NotificationType.Warning);
            return true;
        }
        if (SelectedServiceStatus.StatusName == "Заплановано" && !DateTimeStartService.HasValue)
        {
            _globalNotificationManager.Show("Для статусу \"Заплановано\" потрібно встановити дату початку обслуговування", NotificationType.Warning);
            return true;
        }
        return false;
    }
    
        
    private void LoadServiceStatuses()
    {
        ServiceStatuses.Clear();
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Заплановано"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Очікує підтвердження"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Очікує матеріали"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "В процессі"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Призупинений"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Виконано"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Скасовано"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Діагностика"});
        ServiceStatuses.Add(new ServiceStatusItem {StatusName = "Передано підряднику"});
    }

    private void LoadServiceTypes()
    {
        ServiceTypes.Clear();
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Щоденне обслуговування"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Періодичне технічне обслуговування"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Сезонне обслуговування"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Калібрування / Перевірка"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Діагностика"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Модернізація / Поліпшення"});
        ServiceTypes.Add(new ServiceTypeItem {TypeName = "Інше"});
    }

    public void OnNavigatedTo(NavigationContext navigationContext)
    { 
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _scopedRegionManager = scopedRegionManager;
        }
        
        if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
        {
            _scopedEventAggregator = scopedEventAggregator;
        }
        
        // Set table name value before loading equipments for selector
        _equipmentTableName = navigationContext.Parameters["EquipmentTableName"].ToString();
        
        LoadServiceStatuses();
        LoadServiceTypes();
        
        // If have parameter ServiceItems - its EditMode! Load service data in model
        if (navigationContext.Parameters["ServiceItems"] is ServiceItem serviceItems)
        {
            IsEditMode = true;

            SelectedEquipment = new EquipmentItem
            {
                EquipmentId = serviceItems.EquipmentId,
                EquipmentInventoryNumber = serviceItems.EquipmentInventoryNumber,
                EquipmentBrand = serviceItems.EquipmentBrand,
                EquipmentModel = serviceItems.EquipmentModel
            };

            DateTimeStartService = serviceItems.StartDate;
            DateTimeEndService = serviceItems.EndDate;
            TimeSpentOnService = serviceItems.Duration;
            ServiceDescription = serviceItems.ServiceDescription;

            SelectedServiceType = ServiceTypes.First(s => s.TypeName == serviceItems.Type);
            SelectedServiceStatus = ServiceStatuses.First(s => s.StatusName == serviceItems.Status);
            
            _serviceId = serviceItems.Id;

            ServiceEquipmentTextBlockVisibility = true;
            
            ServiceObjectDisplay = $"{SelectedEquipment.EquipmentInventoryNumber} | {SelectedEquipment.EquipmentBrand} | {SelectedEquipment.EquipmentModel}";
            
            MainButtonContent = "Зберегти";
        }
        
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