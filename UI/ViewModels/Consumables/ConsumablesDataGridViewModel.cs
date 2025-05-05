using System.Collections.ObjectModel;
using System.Dynamic;
using System.Text;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Events.DataGrid.Consumables;
using Core.Services.Common.DataGridColumns;
using Core.Services.Consumables;
using Syncfusion.UI.Xaml.Grid;
using UI.ViewModels.TabControl;

namespace UI.ViewModels.Consumables
{
    public class ConsumablesDataGridViewModel : BindableBase, INavigationAware, IRegionCleanup
    {
        private readonly IAppLogger<ConsumablesDataGridViewModel> _logger;
        private readonly IConsumablesDataGridService _service;
        private readonly IDataGridColumnsService _columnsService;
        private readonly IRegionManager _regionManager;
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
        
        private ObservableCollection<dynamic> _items = new();
        private dynamic _selectedItem;
        public ObservableCollection<dynamic> Items
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
            IRegionManager regionManager,
            IEventAggregator eventAggregator)
        {
            _logger = logger;
            _service = service;
            _columnsService = columnsService;
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;

            LoadedUserControlCommand = new DelegateCommand(OnLoadedUserControl);
            UnloadedUserControlCommand = new DelegateCommand(OnUnloadedUserControl);
        }

        private void OnLoadedUserControl()
        {
            _eventAggregator.GetEvent<AddNewOperationEvent>().Subscribe(ChangeQuantityValue);
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
                if (SelectedItem is IDictionary<string, object> dict)
                {
                    if (dict.TryGetValue("id", out var idObj) && idObj is int id)
                    {
                        _selectedItemId = id;

                        // Show selected record operations
                        _eventAggregator.GetEvent<OnSelectionRecordChanged>().Publish(new SelectionRecordChangedEventArgs
                        {
                            MaterialId = id,
                            TableName = $"{_tableName} операції"
                        });
                        return; 
                    }
                    else
                    {
                        _logger.LogError("Вибраний елемент не містить або містить некоректний 'id'.");
                    }
                }
                else
                {
                    _logger.LogError($"Вибраний елемент має неочікуваний тип: {SelectedItem.GetType().FullName}");
                }
            }
            else
            {
                _logger.LogInformation("Жоден елемент не вибрано.");
            }
        }

        private async void OnConsumablesDataGridContextMenuLoaded()
        {
            if (SelectedItem != null)
            {
                if (SelectedItem is IDictionary<string, object> dict && dict.TryGetValue("id", out var idObj) &&
                    idObj is int id)
                {
                    _selectedItemId = id;
                    ContextMenuLevelIndicatorVisibility = true;

                    var (min, max) = await _service.GetDataMinMaxAsync(_tableName, id);
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
               var data = await _service.GetDataAsync(_tableName);
               Items.Clear();

               foreach (var item in data)
               {
                   if (item is not IDictionary<string, object?> dictionary)
                       continue;
                   if (dictionary.TryGetValue("id", out var idValue) && idValue is int id)
                   {
                       string name = dictionary.TryGetValue("Назва", out var nm) ? nm?.ToString() : null;
                       string category = dictionary.TryGetValue("Категорія", out var ctgr) ? ctgr?.ToString() : null;
                       string quantity = dictionary.TryGetValue("Залишок", out var qnt) ? qnt?.ToString() : null;
                       string minQuantity = dictionary.TryGetValue("Мінімальний залишок", out var qntMin) ? qntMin?.ToString() : null;
                       string maxQuantity = dictionary.TryGetValue("Максимальний залишок", out var qntMax) ? qntMax?.ToString() : null;

                       if (!double.TryParse(quantity, out var numericQuantity))
                           continue;

                       dynamic expando = new ExpandoObject();

                       expando.id = idValue;
                       
                       expando.Name = name;
                       expando.NameDisplay = name;

                       expando.Category = category;
                       expando.CategoryDisplay = category;

                       expando.Balance = numericQuantity;
                       expando.BalanceDisplay = quantity;
                       expando.BalanceWidth = 0;  
                       expando.BalanceColor = "Gray"; 
                       expando.IsCritical = false; 
                       if (double.TryParse(minQuantity, out var min) && double.TryParse(maxQuantity, out var max))
                       {
                           expando.BalanceWidth = BalanceToCellWidth(minQuantity, maxQuantity, quantity);
                           
                           double percent = (expando.BalanceWidth / 200.0) * 100;
                           expando.IsCritical = percent < 15;
                       }
                       else
                       {
                           expando.IsCritical = false;
                       }
                       expando.BalanceColor = WidthColorSelector(expando.BalanceWidth, 200);
                       
                       Items.Add(expando);
                   }
               }
                
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

        private double BalanceToCellWidth(string minValue, string maxValue, string cellValue)
        {
            if (!double.TryParse(minValue, out double min) ||
                !double.TryParse(maxValue, out double max) ||
                !double.TryParse(cellValue, out double value) ||
                max == min)
            {
                return 0;
            }

            const double maxWidth = 200.0;
            double percent = (value - min) / (max - min);
            percent = Math.Clamp(percent, 0, 1);
            
            Console.WriteLine(percent * maxWidth);
            
            return percent * maxWidth;
        }

        private string WidthColorSelector(double width, double maxWidth)
        {
            if (maxWidth <= 0) return "Gray"; 
            
            double percent = (width / maxWidth) * 100;

            if (percent < 15)
                return "Red";
            else if (percent < 50)
                return "DarkGoldenrod";
            else
                return "Green";
        }

        private async void ChangeQuantityValue(AddNewOperationEventArgs args)
        {
            await _service.UpdateQuantityAsync(_tableName, _selectedItemId, args.Quantity, args.OperationType);
            await LoadDataAsync();
        }
        
        private async void OnDataChanged(string payload)
        {
            if (payload.Contains(_tableName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Data changed for table {TableName}: {Payload}", _tableName, payload);
                await LoadDataAsync();
            }
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Console.WriteLine("OnNavigatedTo ConsumablesDataGridViewModel");
            _tableName = navigationContext.Parameters["parameter"] as string;
            _regionManager.RequestNavigate("DetailsConsumablesRegion", "DetailsConsumablesView");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
        public void CleanupRegions()
        {
            _regionManager.Regions.Remove("AddNewOperationTemplateRegion");
            _regionManager.Regions.Remove("DetailsConsumablesRegion");
            _regionManager.Regions.Remove("OperationsConsumablesRegion");
        }
    }
}
