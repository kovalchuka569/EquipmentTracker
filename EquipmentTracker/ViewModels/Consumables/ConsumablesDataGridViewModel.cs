using System.Collections.ObjectModel;
using System.Drawing;
using Common.Logging;
using Core.Events.DataGrid.Consumables;
using Core.Events.TabControl;
using Core.Services.Consumables;
using Microsoft.Win32;
using Models.ConsumablesDataGrid;
using Syncfusion.Pdf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Converter;
using Syncfusion.XlsIO;

namespace UI.ViewModels.Consumables
{
    public class ConsumablesDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IAppLogger<ConsumablesDataGridViewModel> _logger;
        private readonly IConsumablesDataGridService _service;
       // private readonly NotificationManager _notificationManager;
        private readonly IEventAggregator _globalEventAggregator;
        private IRegionManager _scopedRegionManager;
        private IEventAggregator _scopedEventAggregator;
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
        private ObservableCollection<string> _unitItems = new();
        private dynamic _selectedItem;
        public ObservableCollection<ConsumableItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public ObservableCollection<string> UnitItems
        {
            get => _unitItems;
            set => SetProperty(ref _unitItems, value);
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
        

        public DelegateCommand LoadedUserControlCommand { get; }
        public DelegateCommand UnloadedUserControlCommand { get; }
        public DelegateCommand<RowValidatingEventArgs> RowValidatingCommand { get; }
        public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand { get; }
        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand PrintCommand { get; }
        public DelegateCommand ExcelExportCommand { get; }
        public DelegateCommand PdfExportCommand { get; }
        public DelegateCommand OpenConsumableEquipmentChartCommand { get; }
        public ConsumablesDataGridViewModel(
            IAppLogger<ConsumablesDataGridViewModel> logger,
            IConsumablesDataGridService service, 
           // NotificationManager notificationManager, 
            IEventAggregator globalEventAggregator)
        {
            _logger = logger;
            _service = service;
           // _notificationManager = notificationManager;
            _globalEventAggregator = globalEventAggregator;
            
            LoadedUserControlCommand = new DelegateCommand(OnLoadedUserControl);
            UnloadedUserControlCommand = new DelegateCommand(OnUnloadedUserControl);
            RowValidatingCommand = new DelegateCommand<RowValidatingEventArgs>(OnRowValidating);
            RowValidatedCommand = new DelegateCommand<RowValidatedEventArgs>(OnRowValidated);
            RefreshCommand = new DelegateCommand(RefreshAsync);
            PrintCommand = new DelegateCommand(OnPrint);
            ExcelExportCommand = new DelegateCommand(OnExcelExport);
            PdfExportCommand = new DelegateCommand(OnPdfExport);
            OpenConsumableEquipmentChartCommand = new DelegateCommand(OnOpenConsumableEquipmentChart);
        }

        private void OnOpenConsumableEquipmentChart()
        {
            _globalEventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = "Test Chart",
                Parameters = new Dictionary<string, object>
                {
                    {"ViewNameToShow", "ConsumablesEquipmentChartView"}
                }
            });
        }

        private void OnPrint()
        {
            _sfDataGrid.ShowPrintPreview();
        }

        private void OnExcelExport()
        {
            try
            {
                var options = new ExcelExportingOptions
                {
                    ExcelVersion = ExcelVersion.Excel2016
                };
            
                var excelEngine = _sfDataGrid.ExportToExcel(_sfDataGrid.View, options);
                var workbook = excelEngine.Excel.Workbooks[0];

                var saveDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx",
                    Title = $"Експорт в Excel: Розхідні матеріали - {_tableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}",
                    FileName = $"Розхідні матеріали - {_tableName}, {DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss")}.xlsx"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    using (var stream = saveDialog.OpenFile())
                    {
                        workbook.SaveAs(stream);
                    }
                 //   _notificationManager.Show("Таблицю успішно експортовано до Excel!", NotificationType.Success);
                }
            }
            catch (Exception e)
            {
              //  _notificationManager.Show("Помилка експорту до Excel!", NotificationType.Error);
                _logger.LogError(e.Message, "Error exporting Excel");
                throw;
            }
        }

        private void OnPdfExport()
        {
            try
            {
                PdfExportingOptions options = new PdfExportingOptions();
                // Delete notes column
                options.ExcludeColumns.Add("Notes");
                options.FitAllColumnsInOnePage = true;
                var pdfGrid = _sfDataGrid.ExportToPdfGrid(_sfDataGrid.View, options);

                using (PdfDocument document = new PdfDocument())
                {
                    var page = document.Pages.Add();
                    pdfGrid.Draw(page, new PointF(0, 0));

                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "PDF files (*.pdf)|*.pdf",
                        Title = $"Експорт в PDF: Розхідні матеріали - {_tableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}",
                        FileName = $"Розхідні матеріали - {_tableName}, {DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        using (var stream = saveDialog.OpenFile())
                        {
                            document.Save(stream);
                        }

                       // _notificationManager.Show("Таблицю успішно експортовано в PDF!", NotificationType.Success);
                    }
                }

            }
            catch (Exception e)
            {
               // _notificationManager.Show("Помилка експорту в PDF!", NotificationType.Error);
                _logger.LogError(e.Message, "Error exporting PDF");
                throw;
            }
        }

        private void OnRowValidating(RowValidatingEventArgs args)
        {
            var rowData = args.RowData as ConsumableItem;
            if(rowData == null) return;

            // Name validating
            if (string.IsNullOrWhiteSpace(rowData.Name))
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Name", "Назва не може бути порожньою");
            }
            else if (rowData.Name.Length > 254)
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Name", "Максимальна довжина 255 символів");
            }
            
            // Category validating
            if (string.IsNullOrWhiteSpace(rowData.Category))
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Category", "Категорія не може бути порожньою");
            }
            else if (rowData.Category.Length > 254)
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Category", "Максимальна довжина 255 символів");
            }
            
            // Unit validating
            if (string.IsNullOrWhiteSpace(rowData.Unit))
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Unit", "Одиниця не може бути порожньою");
            }
            else if (rowData.Unit.Length > 254)
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Unit", "Максимальна довжина 255 символів");
            }
            
            // Notes validating
            if (!string.IsNullOrEmpty(rowData.Notes) && rowData.Notes.Length > 2000)
            {
                args.IsValid = false;
                args.ErrorMessages.Add("Notes", "Максимальна довжина 2000 символів");
            }
        }

        private async void OnRowValidated(RowValidatedEventArgs args)
        {
            var rowData = args.RowData as ConsumableItem;
            if (rowData == null) return;
            
            // Verifying an add or update operation
            if (rowData.Id == 0)
            {
                // Inserting new consumable
                await _service.InsertConsumableAsync(rowData, _tableName);
                RefreshAsync();
            }
            else
            {
                // Updating new consumable
                await _service.UpdateConsumableAsync(rowData, _tableName);
            }
        }

        private void OnLoadedUserControl()
        {
            _scopedEventAggregator.GetEvent<AddNewOperationEvent>().Subscribe(ChangeQuantityValue);

            var parameters = new NavigationParameters();
            parameters.Add("ScopedRegionManager", _scopedRegionManager);
            parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
            
            _scopedRegionManager.RequestNavigate("DetailsConsumablesRegion", "DetailsConsumablesView", parameters);
        }

        private void OnUnloadedUserControl()
        {
            _scopedEventAggregator.GetEvent<AddNewOperationEvent>().Unsubscribe(ChangeQuantityValue);
        }

        private async void OnMinLevelChanged()
        {
            await _service.UpdateMinLevelAsync(_tableName, _selectedItemId, MinLevel);
            await LoadDataAsync();
        }

        private async void RefreshAsync()
        {
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
                        _scopedEventAggregator.GetEvent<OnSelectionRecordChanged>().Publish(
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

        private void LoadUnits()
        {
            UnitItems = new ObservableCollection<string>
            {
                "шт",
                "кг",
                "г",
                "м",
                "м\u00b2",
                "пара",
                "упаковка",
                "комплект",
                "л",
                "мл",
            };
        }
        private async void OnSfDataGridLoaded(SfDataGrid sfDataGrid)
        {
            try
            {
                _sfDataGrid = sfDataGrid;
                _logger.LogInformation("DataGrid loaded for table {TableName}", _tableName);
                
                await LoadDataAsync();
                LoadUnits();
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
                _scopedRegionManager = scopedRegionManager;
            }
            if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
            {
                _scopedEventAggregator = scopedEventAggregator;
            }
            _tableName = navigationContext.Parameters["TableName"] as string;
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
