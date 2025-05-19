using System.Collections.ObjectModel;
using Common.Logging;
using Core.Events.TabControl;
using Core.Services.RepairsDataGrid;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using UI.ViewModels.TabControl;

namespace UI.ViewModels.DataGrid
{
    public class RepairsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IAppLogger<RepairsDataGridViewModel> _logger;
        private readonly RepairsDataGridService _service;
        private IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        
        private SfDataGrid _repairsDataGrid;

        private string _repairsTableName;
        private string _equipmentTableName;
        
        private ObservableCollection<RepairItem> _repairs = new();
        private RepairItem _selectedRepair;
        
        private bool _progressBarVisibility;

        public ObservableCollection<RepairItem> Repairs
        {
            get => _repairs;
            set => SetProperty(ref _repairs, value);
        }
        public RepairItem SelectedRepair
        {
            get => _selectedRepair;
            set => SetProperty(ref _selectedRepair, value);
        }
        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }
        
        public DelegateCommand<SfDataGrid> RepairsDataGridLoadedCommand { get; set; }
        public DelegateCommand CreateNewRepairCommand { get; set; }
        
        public RepairsDataGridViewModel(IAppLogger<RepairsDataGridViewModel> logger, 
            RepairsDataGridService service,
            IEventAggregator eventAggregator)
        {
            _logger = logger;
            _service = service;
            _eventAggregator = eventAggregator;

            RepairsDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnRepairsDataGridLoaded);
            CreateNewRepairCommand = new DelegateCommand(OnCreateNewRepair);
        }

        private void OnCreateNewRepair()
        {
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = _equipmentTableName + " створення ремонту",
                Parameters = new Dictionary<string, object>
                {
                    {"ViewNameToShow", "AddRepairView"},
                    {"AddRepairView.EquipmentTableName", _equipmentTableName}
                }
            });
        }

        private async Task LoadRepairsDataAsync()
        {
            ProgressBarVisibility = true;
            await Task.Delay(1000);
            Repairs = await _service.GetRepairItems(_repairsTableName, _equipmentTableName);
            ProgressBarVisibility = false;
        }

        private void OnRepairsDataGridLoaded(SfDataGrid repairsDataGrid)
        {
            _repairsDataGrid = repairsDataGrid;
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
            {
                _regionManager = scopedRegionManager;
            }
            _repairsTableName = navigationContext.Parameters["RepairsTableName"] as string;
            _equipmentTableName = _repairsTableName.EndsWith("Р") ? _repairsTableName.Substring(0, _repairsTableName.Length - 1).TrimEnd() : _repairsTableName;

            LoadRepairsDataAsync();
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

