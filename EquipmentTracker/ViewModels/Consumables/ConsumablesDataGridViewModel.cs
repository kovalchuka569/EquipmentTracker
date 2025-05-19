using System.Collections.ObjectModel;
using System.Dynamic;
using System.Text;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Events.DataGrid.Consumables;
using Core.Services.Common.DataGridColumns;
using Core.Services.Consumables;
using Models.ConsumablesDataGrid;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using UI.ViewModels.TabControl;

namespace UI.ViewModels.Consumables
{
    public class ConsumablesDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IAppLogger<ConsumablesDataGridViewModel> _logger;
        private readonly IConsumablesDataGridService _service;
        private readonly IDataGridColumnsService _columnsService;
        private IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        private CancellationTokenSource _cts;
        private SubscriptionToken _dataChangedToken;
        
        private bool _isLoading;
        private bool _contextMenuLevelIndicatorVisibility = false;
        private int? _minLevel;
        private int? _maxLevel;
        private int _selectedItemId;

        private string _uniqueRegionName;
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public bool ContextMenuLevelIndicatorVisibility
        {
            get => _contextMenuLevelIndicatorVisibility;
            set => SetProperty(ref _contextMenuLevelIndicatorVisibility, value);
        }

        public int? MinLevel
        {
            get => _minLevel;
            set => SetProperty(ref _minLevel, value);
        }

        public int? MaxLevel
        {
            get => _maxLevel;
            set => SetProperty(ref _maxLevel, value);
        }

        public string UniqueRegionName
        {
            get => _uniqueRegionName;
            set => SetProperty(ref _uniqueRegionName, value);
        }
        
        private SfDataGrid _sfDataGrid;
        private string _tableName;
        
        private ObservableCollection<ConsumableItem> _items = new();
        private dynamic _selectedItem;
        public ObservableCollection<ConsumableItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public dynamic SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        
        private DelegateCommand<SfDataGrid> _loadedSfDataGridCommand;
        private DelegateCommand _consumablesDataGridContextMenuLoadedCommand;
        private DelegateCommand _minLevelChangedCommand;
        private DelegateCommand _maxLevelChangedCommand;
        private DelegateCommand _rowSelectionChangedCommand;
        public DelegateCommand<SfDataGrid> LoadedSfDataGridCommand =>
            _loadedSfDataGridCommand ??= new DelegateCommand<SfDataGrid>(OnSfDataGridLoaded);

        public DelegateCommand ConsumablesDataGridContextMenuLoadedCommand =>
            _consumablesDataGridContextMenuLoadedCommand ??=
                new DelegateCommand(OnConsumablesDataGridContextMenuLoaded);

        public DelegateCommand MinLevelChangedCommand =>
            _minLevelChangedCommand ??= new DelegateCommand(OnMinLevelChanged);

        public DelegateCommand MaxLevelChangedCommand =>
            _maxLevelChangedCommand ??= new DelegateCommand(OnMaxLevelChanged);

        public DelegateCommand RowSelectionChangedCommand =>
            _rowSelectionChangedCommand ??= new DelegateCommand(OnSelectionRowChanged);

        private DelegateCommand _refreshCommand;
        public DelegateCommand RefreshCommand =>
            _refreshCommand ??= new DelegateCommand(async () => await LoadDataAsync(), () => !IsLoading);


        public DelegateCommand LoadedUserControlCommand { get; }
        public DelegateCommand UnloadedUserControlCommand { get; }
        public ConsumablesDataGridViewModel(
            IAppLogger<ConsumablesDataGridViewModel> logger,
            IConsumablesDataGridService service,
            IDataGridColumnsService columnsService,
            IEventAggregator eventAggregator)
        {
            _logger = logger;
            _service = service;
            _columnsService = columnsService;
            _eventAggregator = eventAggregator;

            LoadedUserControlCommand = new DelegateCommand(OnLoadedUserControl);
            UnloadedUserControlCommand = new DelegateCommand(OnUnloadedUserControl);
        }

        private void OnLoadedUserControl()
        {
            _eventAggregator.GetEvent<AddNewOperationEvent>().Subscribe(ChangeQuantityValue);

            var parameters = new NavigationParameters();
            parameters.Add("ScopedRegionManager", _regionManager);
            
            _regionManager.RequestNavigate("DetailsConsumablesRegion", "DetailsConsumablesView", parameters);
        }

        private void OnUnloadedUserControl()
        {
            _eventAggregator.GetEvent<AddNewOperationEvent>().Unsubscribe(ChangeQuantityValue);
        }

        private async void OnMinLevelChanged()
        {
            await _service.UpdateMinLevelAsync(_tableName, _selectedItemId, MinLevel);
            await LoadDataAsync();
        }

        private async void OnMaxLevelChanged()
        {
            await _service.UpdateMaxLevelAsync(_tableName, _selectedItemId, MaxLevel);
            await LoadDataAsync();
        }

        private void OnSelectionRowChanged()
        {
            if (SelectedItem != null)
            {
                if (SelectedItem is ConsumableItem consumable)
                {
                        _selectedItemId = consumable.Id;
                        
                        // Show selected record operations
                        _eventAggregator.GetEvent<OnSelectionRecordChanged>().Publish(
                            new SelectionRecordChangedEventArgs
                            {
                                MaterialId = consumable.Id,
                                OperationsTableName = $"{_tableName} операції",
                                TableName = _tableName,
                            });
                }
            }
        }

        private async void OnConsumablesDataGridContextMenuLoaded()
        {
            if (SelectedItem != null)
            {
                if (SelectedItem is ConsumableItem consumable)
                {
                    _selectedItemId = consumable.Id;
                    ContextMenuLevelIndicatorVisibility = true;

                    var (min, max) = await _service.GetDataMinMaxAsync(_tableName, consumable.Id);
                    MinLevel = min;
                    MaxLevel = max;

                }
            }
        }

        private async void OnSfDataGridLoaded(SfDataGrid sfDataGrid)
        {
            try
            {
                _sfDataGrid = sfDataGrid;
                _logger.LogInformation("DataGrid loaded for table {TableName}", _tableName);
                
                await LoadDataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing DataGrid for table {TableName}", _tableName);
            }
        }

        private async Task LoadDataAsync()
        {
            if (IsLoading) return;
            
            try
            {
                Items = await _service.GetDataAsync(_tableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading data for table {TableName}", _tableName);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ChangeQuantityValue(AddNewOperationEventArgs args)
        {
            await _service.UpdateQuantityAsync(_tableName, _selectedItemId, args.Quantity, args.OperationType);
            await LoadDataAsync();
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
            {
                _regionManager = scopedRegionManager;
            }
            _tableName = navigationContext.Parameters["TableName"] as string;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
