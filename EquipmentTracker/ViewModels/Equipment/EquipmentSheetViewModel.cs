using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows;
using Common.Logging;
using Core.Common.RegionHelpers;
using Core.Services.EquipmentDataGrid;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Factories.Interfaces;
using Models.Constants;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Notification.Wpf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace EquipmentTracker.ViewModels.Equipment;

public class EquipmentSheetViewModel : BindableBase, INavigationAware, IDestructible, IRegionMemberLifetime
{
    private readonly IAppLogger<EquipmentSheetViewModel> _logger;
    private readonly NotificationManager _notificationManager;
    private IEquipmentSheetService _service;
    private readonly IGridColumnFactory _columnFactory;

    private IRegionManager? _scopedRegionManager;
    private IEventAggregator? _scopedEventAggregator;
    private Guid _equipmentSheetId;
    private bool _isInitialized;

    public EquipmentSheetViewModel(
        IAppLogger<EquipmentSheetViewModel> logger,
        NotificationManager notificationManager,
        IGridColumnFactory columnFactory)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _columnFactory = columnFactory;
        
        InitializeCommands();
    }
    
    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    #region Properties

    private ObservableCollection<ExpandoObject> _rows = new();
    public ObservableCollection<ExpandoObject> Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }
    
    private Columns _columns = new();
    public Columns Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    private ObservableCollection<object> _selectedItems = new();
    public ObservableCollection<object> SelectedItems
    {
        get => _selectedItems;
        set => SetProperty(ref _selectedItems, value);
    }

    private bool _isOverlayVisible;
    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }

    private bool _isLoading;
    public bool IsLoading
    {
        get => _isLoading;
        set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
                RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            }
        }
    }
    
    public bool KeepAlive => true;

    private SfDataGrid _dataGrid;

    public bool ColumnsEmptyTipVisibility => !IsLoading && !Columns.Any();
    public bool RowsEmptyTipVisibility => !IsLoading && Columns.Any() && !Rows.Any();
    public bool DeleteRowContextMenuItemVisibility => SelectedItems.Any();

    #endregion

    #region Commands

    public DelegateCommand AddColumnCommand { get; private set; }
    public DelegateCommand<SfDataGrid> DataGridLoadedCommand { get; private set; }

    private void InitializeCommands()
    {
        AddColumnCommand = new DelegateCommand(OnAddColumn);
        DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
    }

    #endregion

    private async void OnDataGridLoaded(SfDataGrid dataGrid)
    {
        _dataGrid = dataGrid;
        
        if (!_isInitialized)
        {
            await LoadDataAsync();
            _isInitialized = true;
        }
    }

    #region Navigation

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (_isInitialized)
        {
            _logger.LogInformation("Already initialized, skipping OnNavigatedTo");
            UpdateOverlayVisibility();
            return;
        }

        try
        {
            GetNavigationParameters(navigationContext.Parameters);
            SubscribeToRegionsChanges();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize EquipmentSheetViewModel");
            throw;
        }
    }

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    #endregion

    #region Data Loading
    
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            var columns = await GetColumnsAsync();
            var rows = await _service.GetRowsByEquipmentSheetIdAsync(_equipmentSheetId);
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _dataGrid.Columns.Suspend();

                _dataGrid.Columns.Clear();
                foreach (var col in columns)
                    _dataGrid.Columns.Add(col);

                _dataGrid.Columns.Resume();
                _dataGrid.RefreshColumns();

                Rows.Clear();

                // Генерируем 100000 строк, каждая с 500 колонками
                for (int rowIndex = 0; rowIndex < 100000; rowIndex++)
                {
                    dynamic expando = new ExpandoObject();
                    var expandoDict = (IDictionary<string, object>)expando;

                    foreach (var col in columns)
                    {
                        // Можно брать данные из rowsData, или генерировать фейковые
                        // Например, здесь просто строка с индексами
                        expandoDict[col.MappingName] = $"R{rowIndex}C{col.MappingName}";
                    }

                    Rows.Add(expando);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load data");
            try
            {
                _notificationManager.Show("Помилка завантаження даних", NotificationType.Error);
            }
            catch { /* ignored */ }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task<Columns> GetColumnsAsync()
    {
        var columnItems = await _service.GetColumnsByEquipmentSheetIdAsync(_equipmentSheetId);
        
        var columns = new Columns();
        foreach (var item in columnItems)
        {
            var column = _columnFactory.CreateColumn(item.Settings);
            columns.Add(column);
        }

        return columns;
    }
    #endregion

    #region Column Management

    private void OnAddColumn()
    {
        if (_scopedRegionManager == null) return;

        var parameters = new NavigationParameters
        {
            { "ScopedRegionManager", _scopedRegionManager },
            { "ColumnCreatedCallback", new Action<ColumnCreationResult>(CreateColumn) }
        };

        _scopedRegionManager.RequestNavigate(
            EquipmentSheetConstants.ColumnCreatorRegion,
            ViewNamesConstants.ColumnCreatorView,
            parameters);
    }

    private async void CreateColumn(ColumnCreationResult result)
    {
        _scopedRegionManager?.Regions[EquipmentSheetConstants.ColumnCreatorRegion].RemoveAll();
        if (!result.IsSuccessful) return;

        try
        {
            IsLoading = true;

            result.ColumnSettings.ColumnPosition = Columns.Count + 1;

            var columnItem = new ColumnItem
            {
                Id = Guid.NewGuid(),
                Settings = result.ColumnSettings,
                EquipmentSheetId = _equipmentSheetId,
            };

            await _service.InsertColumnAsync(columnItem);

            var newColumn = _columnFactory.CreateColumn(result.ColumnSettings);
            if (newColumn != null)
            {
                Columns.Add(newColumn);
                _notificationManager.Show("Характеристика успішно створена", NotificationType.Success);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create column");
            _notificationManager.Show("Помилка створення характеристики", NotificationType.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    #endregion

    #region Private Methods

    private void GetNavigationParameters(INavigationParameters parameters)
    {
        var tabScopedServiceProvider = parameters.GetValue<IScopedProvider>("TabScopedServiceProvider");
        _service = tabScopedServiceProvider.Resolve<IEquipmentSheetService>();
        _scopedRegionManager ??= parameters[NavigationConstants.ScopedRegionManager] as IRegionManager;
        _scopedEventAggregator ??= parameters[NavigationConstants.ScopedEventAggregator] as IEventAggregator;
        _equipmentSheetId = (Guid)(parameters[EquipmentSheetConstants.EquipmentSheetId] ?? Guid.Empty);
    }

    private void SubscribeToRegionsChanges()
    {
        if (_scopedRegionManager == null) return;

        _scopedRegionManager.Regions[EquipmentSheetConstants.ColumnCreatorRegion]
            .ActiveViews.CollectionChanged += OnActiveViewsChanged;

        _scopedRegionManager.Regions[EquipmentSheetConstants.ExcelSheetSelectorRegion]
            .ActiveViews.CollectionChanged += OnActiveViewsChanged;

        UpdateOverlayVisibility();
    }

    private void OnActiveViewsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateOverlayVisibility();
    }

    private void UpdateOverlayVisibility()
    {
        if (_scopedRegionManager == null) return;

        IsOverlayVisible =
            _scopedRegionManager.Regions[EquipmentSheetConstants.ColumnCreatorRegion].ActiveViews.Any() ||
            _scopedRegionManager.Regions[EquipmentSheetConstants.ExcelSheetSelectorRegion].ActiveViews.Any();
    }

    #endregion

    public async void Destroy()
    {
        try
        {
            if (_scopedRegionManager != null)
            {
                RegionCleanupHelper.CleanRegion(_scopedRegionManager, EquipmentSheetConstants.ColumnCreatorRegion);
                RegionCleanupHelper.CleanRegion(_scopedRegionManager, EquipmentSheetConstants.ExcelSheetSelectorRegion);
            }

            Console.WriteLine("Destroy from equipmentsheetviewmodel");
            await _service.DisposeAsync();

            _dataGrid = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy data");
        }
    }
}
