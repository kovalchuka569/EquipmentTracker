using System.Collections.ObjectModel;
using System.Windows.Forms;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Core.Events.DataGrid.Consumables;
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
    private SfMultiColumnDropDownControl _equipmentSelector;
    private readonly IAddRepairService _service;
    private readonly IEventAggregator _eventAggregator;
    
    private string _equipmentTableName;
    
    private ObservableCollection<EquipmentItem> _equipments = new();
    private EquipmentItem _selectedEquipment;
    private List<RepairStatusItem> _repairStatuses = new();
    private RepairStatusItem _selectedRepairStatus;

    #region Fields

    private DateTime? _dateTimeStartRepair;
    private DateTime? _dateTimeEndRepair;
    private TimeSpan? _timeSpentOnRepair;
    private string? _breakDescription;
    

    #endregion
    
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
    
    
    public DelegateCommand UserControlLoadedCommand { get; }
    public DelegateCommand<SfMultiColumnDropDownControl> EquipmentSelectorLoadedCommand { get; }
    public DelegateCommand SaveRepairCommand { get; }
    public AddRepairViewModel(IAddRepairService service,
        IEventAggregator eventAggregator,
        NotificationManager notificationManager)
    {
        _service = service;
        _eventAggregator = eventAggregator;
        _notificationManager = notificationManager;
        
        UserControlLoadedCommand = new DelegateCommand(OnUserControlLoaded);
        EquipmentSelectorLoadedCommand = new DelegateCommand<SfMultiColumnDropDownControl>(OnEquipmentSelectorLoaded);
        SaveRepairCommand = new DelegateCommand(OnSaveRepair);
        
        DateTimeStartRepair = DateTime.Now;
        DateTimeEndRepair = null;
        TimeSpentOnRepair = null;
    }
    
    private void OnUserControlLoaded()
    {
        var parameters = new NavigationParameters();
        parameters.Add("ScopedRegionManager", _regionManager);
        _regionManager.RequestNavigate("DataGridUsedMaterialsRegion", "DataGridUsedMaterialsView", parameters);
    }
    

    private void OnSaveRepair()
    {
        if (HaveNullData()) return;
        
        var newRepairData = new RepairData
        {
            EquipmentId = SelectedEquipment.EquipmentId,
            RepairTableName = _equipmentTableName + " Р",
            StartRepair = DateTimeStartRepair,
            EndRepair = DateTimeEndRepair,
            TimeSpentOnRepair = TimeSpentOnRepair,
            BreakDescription = BreakDescription,
            RepairStatus = SelectedRepairStatus.StatusName
        };
    }

    private bool HaveNullData()
    {
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

    private void OnEquipmentSelectorLoaded(SfMultiColumnDropDownControl equipmentSelector)
    {
        _equipmentSelector = equipmentSelector;
    }
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        _equipmentTableName = navigationContext.Parameters["EquipmentTableName"].ToString();
        LoadEquipmentsAsync();
        LoadRepairStatuses();
    }
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}