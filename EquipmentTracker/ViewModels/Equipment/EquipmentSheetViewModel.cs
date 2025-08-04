using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using Core.Common.EquipmentSheetValidation;
using Core.Common.EquipmentSheetValidation.CellValidator;
using Core.Common.EquipmentSheetValidation.RowValidator;
using Core.Common.RegionHelpers;
using Core.Services.EquipmentDataGrid;
using EquipmentTracker.Common;
using EquipmentTracker.Common.Controls;
using EquipmentTracker.Common.DataGridExport;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Factories.Interfaces;
using EquipmentTracker.ViewModels.Common.Table;
using Models.Common.Table;
using Models.Common.Table.ColumnSpecificSettings;
using Models.Common.Table.ColumnValidationRules;
using Models.Constants;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Notification.Wpf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using IDialogService = EquipmentTracker.Common.DialogService.IDialogService;

namespace EquipmentTracker.ViewModels.Equipment;

public class EquipmentSheetViewModel : BindableBase, INavigationAware, IDestructible, IRegionMemberLifetime
{
    private readonly IAppLogger<EquipmentSheetViewModel> _logger;
    private readonly NotificationManager _notificationManager;
    private IEquipmentSheetService _service;
    private IDialogService _dialogService;
    private readonly IGridColumnFactory _columnFactory;
    private readonly CellValidator _cellValidator = new();
    private readonly RowValidator _rowValidator = new();

    private IRegionManager? _scopedRegionManager;
    private IEventAggregator? _scopedEventAggregator;
    private Guid _equipmentSheetId;
    private bool _isInitialized;

    public EquipmentSheetViewModel(
        IAppLogger<EquipmentSheetViewModel> logger,
        NotificationManager notificationManager,
        IGridColumnFactory columnFactory,
        IDialogService dialogService)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _columnFactory = columnFactory;
        _dialogService = dialogService;

        InitializeCommands();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    #region Properties

    private ObservableCollection<RowViewModel> _rowViewModels = new();
    public ObservableCollection<RowViewModel> RowViewModels
    {
        get => _rowViewModels;
        set => SetProperty(ref _rowViewModels, value);
    }

    private ObservableCollection<ExpandoObject> _rows = new();
    public ObservableCollection<ExpandoObject> Rows
    {
        get => _rows;
        set => SetProperty(ref _rows, value);
    }
    
    // Meta of row id, dictionary of mappingName - cell id
    private record RowMeta(Guid RowId, Dictionary<string, Guid> CellIds);
    
    private readonly Dictionary<ExpandoObject, RowMeta> _rowMetaMap = new();

    // TODO: Make the class of type ColumnInfo to store all these dictionaries (optional)

    private readonly Dictionary<Guid, ColumnModel> _columnIdModelMap = new();
    private readonly Dictionary<Guid, string> _columnIdHeaderTextMap = new();
    private readonly Dictionary<Guid, IColumnSpecificSettings> _columnIdSpecificSettingsMap = new();
    private readonly Dictionary<Guid, ColumnDataType> _columnIdDataTypeMap = new();
    private readonly Dictionary<Guid, IColumnValidationRules> _columnIdValidationRulesMap = new();
    private readonly Dictionary<string, Guid> _columnMappingNameIdMap = new();
    
    private Columns _columns = new();
    public Columns Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    private int _frozenColumnCount;
    public int FrozenColumnCount
    {
        get => _frozenColumnCount;
        set => SetProperty(ref _frozenColumnCount, value);
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
            if (!SetProperty(ref _isLoading, value)) return;
            
            RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
        }
    }


    public bool KeepAlive => true;

    private Style _gridHeaderBasedStyle = new();
    private SfDataGrid _dataGrid = new();

    public bool ColumnsEmptyTipVisibility => !IsLoading && !Columns.Any();
    public bool RowsEmptyTipVisibility => !IsLoading && Columns.Any() && !Rows.Any();
    public bool DeleteRowContextMenuItemVisibility => SelectedItems.Any();

    #endregion

    #region Commands
    
    public DelegateCommand<SfDataGrid> DataGridLoadedCommand { get; private set; }
    
    public DelegateCommand ExportToExcelCommand { get; private set; }
    public DelegateCommand ExportToPdfCommand { get; private set; }
    public DelegateCommand PrintCommand { get; private set; }
    
    public DelegateCommand<RowValidatingEventArgs> RowValidatingCommand { get; private set; }
    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand { get; private set; }
    public DelegateCommand<CurrentCellValidatingEventArgs> CurrentCellValidatingCommand { get; private set; }
    public DelegateCommand<CurrentCellBeginEditEventArgs> CurrentCellBeginEditCommand { get; private set; }
    public DelegateCommand<CurrentCellValueChangedEventArgs> CurrentCellValueChangedCommand { get; private set; }
    public DelegateCommand<CurrentCellEndEditEventArgs> CurrentCellEndEditCommand { get; private set; }
    public DelegateCommand<CurrentCellValidatedEventArgs> CurrentCellValidatedCommand { get; private set; }
    public DelegateCommand<AddNewRowInitiatingEventArgs> AddNewRowInitiatingCommand { get; private set; }
    public DelegateCommand<GridFilterItemsPopulatedEventArgs> FilterItemsPopulatedCommand { get; private set; }
    public DelegateCommand AddColumnCommand { get; private set; }
    public DelegateCommand<GridColumnContextMenuInfo> RemoveColumnCommand { get; private set; }
    public DelegateCommand<GridColumnContextMenuInfo> EditColumnCommand { get; private set; }
    public DelegateCommand RemoveRowCommand { get; private set; }
    public DelegateCommand<RecordDeletingEventArgs> KeyRemoveRowCommand { get; private set; }
    public DelegateCommand<GridSelectionChangedEventArgs> SelectionChangedCommand { get; private set; }
    public DelegateCommand<QueryColumnDraggingEventArgs> QueryColumnDraggingCommand { get; private set; }
    public DelegateCommand<KeyEventArgs> DataGridKeyDownCommand { get; private set; }

    private void InitializeCommands()
    {
        DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
        
        ExportToExcelCommand = new DelegateCommand(OnExportToExcel);
        ExportToPdfCommand = new DelegateCommand(OnExportToPdf);
        PrintCommand = new DelegateCommand(OnPrint);

        RowValidatingCommand = new DelegateCommand<RowValidatingEventArgs>(OnRowValidating);
        RowValidatedCommand = new DelegateCommand<RowValidatedEventArgs>(OnRowValidated);
        CurrentCellValidatingCommand = new DelegateCommand<CurrentCellValidatingEventArgs>(OnCurrentCellValidating);
        
        AddColumnCommand = new DelegateCommand(OnAddColumn);
        RemoveColumnCommand = new DelegateCommand<GridColumnContextMenuInfo>(OnRemoveColumn);
        
        CurrentCellBeginEditCommand = new DelegateCommand<CurrentCellBeginEditEventArgs>(OnCurrentCellBeginEdit);
        CurrentCellValueChangedCommand = new DelegateCommand<CurrentCellValueChangedEventArgs>(OnCurrentCellValueChanged);
        
        AddNewRowInitiatingCommand = new DelegateCommand<AddNewRowInitiatingEventArgs>(OnAddNewRowInitiating);
        FilterItemsPopulatedCommand = new DelegateCommand<GridFilterItemsPopulatedEventArgs>(OnGridFilterItemsPopulated);
        EditColumnCommand = new DelegateCommand<GridColumnContextMenuInfo>(OnEditColumn);
        
        RemoveRowCommand = new DelegateCommand(OnRemoveRow);
        KeyRemoveRowCommand = new DelegateCommand<RecordDeletingEventArgs>(OnKeyRemoveRow);
        SelectionChangedCommand = new DelegateCommand<GridSelectionChangedEventArgs>(OnSelectionChanged);
        QueryColumnDraggingCommand = new DelegateCommand<QueryColumnDraggingEventArgs>(OnQueryColumnDragging);
        CurrentCellValidatedCommand = new DelegateCommand<CurrentCellValidatedEventArgs>(OnCurrentCellValidated);
        CurrentCellEndEditCommand = new DelegateCommand<CurrentCellEndEditEventArgs>(OnCurrentCellEndEdit);
        DataGridKeyDownCommand = new DelegateCommand<KeyEventArgs>(OnDataGridGridKeyDown);
    }

    #endregion
    

    private void OnDataGridGridKeyDown(KeyEventArgs args)
    {

    }

    private void OnCurrentCellEndEdit(CurrentCellEndEditEventArgs args)
    {
    }

    private void OnCurrentCellValidated(CurrentCellValidatedEventArgs args)
    {
    }

    private async void OnQueryColumnDragging(QueryColumnDraggingEventArgs args)
    {
        if (args.Reason == QueryColumnDraggingReason.Dropped)
        {
            // Dictionary column new positions, where ID - new position
            var newColumnPositions = new Dictionary<Guid, int>();

            for (var i = 0; i < _dataGrid.Columns.Count; i++)
            {
                var column = _dataGrid.Columns[i];
                var columnId = _columnMappingNameIdMap[column.MappingName];
                newColumnPositions[columnId] = i;
                var columnModel = _columnIdModelMap[columnId];
                columnModel.Order = i;
            }

            try
            {
                await _service.UpdateColumnsPositionsAsync(_equipmentSheetId, newColumnPositions);
            }
            catch (Exception e)
            {
                _notificationManager.Show("Помилка оновлення позицій характеристик", NotificationType.Error);
                _logger.LogError(e, "Error updating columns positions");
                throw;
            }
        }
    }
    
    private void OnSelectionChanged(GridSelectionChangedEventArgs args)
    {
        RaisePropertyChanged(nameof(DeleteRowContextMenuItemVisibility));
    }

    private async void OnKeyRemoveRow(RecordDeletingEventArgs args)
    {
        try
        {
            var confirm = await ProcessRemovalRow();
            args.Cancel = !confirm;
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка видалення записів", NotificationType.Error);
            _logger.LogError(e, "Error removing rows");
        }
    }
    
    private async void OnRemoveRow()
    {
        try
        {
            await ProcessRemovalRow();
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка видалення записів", NotificationType.Error); 
            _logger.LogError(e, "Error removing rows");
        }
    }
    
    private async Task<bool> ProcessRemovalRow()
    {
        using var cts = new CancellationTokenSource();

        var rowsToRemove = SelectedItems.Cast<ExpandoObject>().ToList();

        var removeItemsCount = rowsToRemove.Count;
        if (removeItemsCount == 0) return false;
        
        var deletedRecordCountText = PluralizedHelper.GetPluralizedText(removeItemsCount, "запис", "записи", "записів");

        var confirm = await RemoveRowAgreement(removeItemsCount, deletedRecordCountText);
        if (!confirm) return false;

        return await ExecuteRemovalRows(rowsToRemove, deletedRecordCountText);
    }
    
    private async Task<bool> ExecuteRemovalRows(List<ExpandoObject> rowsToRemove, string deletedRecordCountText)
    {
        var rowIdsToRemove = rowsToRemove
            .Select(x => _rowMetaMap[x].RowId)
            .ToList();
        
        try
        {
            await _service.SoftRemoveRowsAsync(_equipmentSheetId, rowIdsToRemove);

            foreach (var row in rowsToRemove)
            {
                Rows.Remove(row);
                _rowMetaMap.Remove(row);
            }

            SelectedItems.Clear();
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            _notificationManager.Show($"Успішно видалено {deletedRecordCountText}", NotificationType.Success);
            return true;
        }
        catch (Exception e)
        {
            _notificationManager.Show($"Помилка видалення {e.Message}", NotificationType.Error);
            _logger.LogError(e, "Error removing equipments");
            return false;
        }
    }
    
    private async Task<bool> RemoveRowAgreement(int removeItemsCount, string deletedRecordCountText)
    {
        if (removeItemsCount == 0) return false;

        string message = removeItemsCount == 1
            ? $"Ви впевнені що хочете видалити цей запис?\nБуде видалено всі комірки для цього запису"
            : $"Ви впевнені що хочете видалити {deletedRecordCountText}?\nБуде видалено всі комірки для цих записів";

        IsOverlayVisible = true;
        try
        {
            return await _dialogService.ShowDeleteConfirmationAsync("Видалення", message);
        }
        finally
        {
            IsOverlayVisible = false;
        }
    }

    // Programmatically change filter item display text for GridCheckBoxColumn when GridFilterItemsPopulated event is raised
    private void OnGridFilterItemsPopulated(GridFilterItemsPopulatedEventArgs e)
    {
        if (e.Column.GetType() != typeof(GridCheckBoxColumn)) return;
        
        foreach (var item in e.ItemsSource)
        {
            if (item is not { ActualValue: bool boolValue }) continue;
            
            item.DisplayText = boolValue ? "Так" : "Ні";
        }
    }

    // Programmatically separately for Grid CheckBox Column we save the row value,
    // since it does not call RowValidating/RowValidated
    private async void OnCurrentCellValueChanged(CurrentCellValueChangedEventArgs e)
    {
        // Enabling the RowValidating event if the changes happen in GridCheckBoxColumn
        if (e.Column.GetType() != typeof(GridCheckBoxColumn)) return;
        
        if (!_isAddingNewRow)
        {
            // ONLY updating cell
            var row =  e.Record as ExpandoObject ?? throw new InvalidOperationException("Row is not expando object");
            var cellId = _rowMetaMap[row].CellIds[e.Column.MappingName];
            var isNewRow = cellId == Guid.Empty;
               
            if (!isNewRow)
            {
                var currentValue = ((IDictionary<string, object?>)row)[e.Column.MappingName];
                bool actualNewValue;
                   
                if (currentValue is bool val)
                {
                    actualNewValue = !val;
                }
                else
                {
                    actualNewValue = false;
                }
                   
                await _service.UpdateCellValueAsync(_equipmentSheetId, cellId, actualNewValue);
            }
               
        }
            
        // Make not valid for activate row validation
        _dataGrid.GetValidationHelper().SetCurrentRowValidated(false);
    }

    private bool _isAddingNewRow;
    private void OnAddNewRowInitiating(AddNewRowInitiatingEventArgs args)
    {
        _isAddingNewRow = true;
        if (args.NewObject is not ExpandoObject expandoObject) return;
        
        foreach (var entry in _columnMappingNameIdMap)
        {
            var mappingName = entry.Key;
            var columnId = entry.Value;
            var dataType = _columnIdDataTypeMap[columnId];
                
            var defaultValue = GetDefaultValue(dataType);
                
            ((IDictionary<string, object?>)expandoObject)[mappingName] = defaultValue;

            if (!_columnIdSpecificSettingsMap.TryGetValue(columnId, out var columnSpecificSettings)) continue;
                
            if (columnSpecificSettings is CheckBoxColumnSpecificSettings checkBoxColumnSpecificSettings)
            {
                ((IDictionary<string, object?>)expandoObject)[mappingName] = checkBoxColumnSpecificSettings.DefaultValue;
            }
        }
    }

    private object? GetDefaultValue(ColumnDataType dataType)
    {
        return dataType switch
        {
            ColumnDataType.Text or ColumnDataType.List or ColumnDataType.Hyperlink or ColumnDataType.Number
                or ColumnDataType.Date or ColumnDataType.Currency => null,
            ColumnDataType.Boolean => false,
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), dataType, null)
        };
    }

    private void OnCurrentCellBeginEdit(CurrentCellBeginEditEventArgs args)
    {
        _isAddingNewRow = false;
    }
    
    private async void OnDataGridLoaded(SfDataGrid dataGrid)
    {
        _dataGrid = dataGrid;
        _gridHeaderBasedStyle = _dataGrid.TryFindResource("BasedGridHeaderStyle") as Style ?? throw new KeyNotFoundException();
        
        SubscribeToRowsDragDropEvents();
        
        if (!_isInitialized)
        {
            await LoadDataAsync();
            _isInitialized = true;
        }
    }

    #region Row & cell validation and save methods

    private void OnRowValidating(RowValidatingEventArgs e)
    {
        var rowValidationArgs = new RowValidationArgs
        {
            CurrentRow = e.RowData as ExpandoObject ?? throw new InvalidOperationException(),
            ColumnMappingNameIdMap = _columnMappingNameIdMap,
            ColumnIdHeaderTextMap = _columnIdHeaderTextMap,
            ColumnIdDataTypeMap = _columnIdDataTypeMap,
            ColumnIdValidationRulesMap = _columnIdValidationRulesMap,
            Rows = Rows.Cast<IDictionary<string, object>>().ToList()
        };

        RowValidationResult rowValidateResult;
        
        try
        {
            rowValidateResult = _rowValidator.ValidateRow(rowValidationArgs);
        }
        catch (Exception ex)
        {
            _notificationManager.Show("Помилка під час валідації рядка");
            _logger.LogError(ex, "Exception occurred during row validation");
            throw;
        }
        
        if (!rowValidateResult.IsValid)
        {
            e.IsValid = false;

            foreach (var kvp in rowValidateResult.ErrorMessages)
            {
                e.ErrorMessages[kvp.Key] = kvp.Value;
            }
        }
        else
        {
            e.IsValid = true;
        }
    }
    
    private async void OnRowValidated(RowValidatedEventArgs e)
    {
        if (e.RowData is not ExpandoObject rowExpando)
            throw new InvalidOperationException("Invalid row data");
        
        var rowDict = rowExpando as IDictionary<string, object>
                      ?? throw new InvalidOperationException("Row expando cast failed");

        if (!_rowMetaMap.TryGetValue(rowExpando, out var meta))
        {
            meta = new RowMeta(Guid.Empty, new Dictionary<string, Guid>());
            _rowMetaMap[rowExpando] = meta;
        }
        
        var rowId = meta.RowId;

        // Creating new row if it doesn't exist
        if (rowId == Guid.Empty)
        {
            var (newRowId, newCellIds) = await _service.InsertRowAsync(_equipmentSheetId, rowDict);
            _rowMetaMap[rowExpando] = new RowMeta(newRowId, newCellIds);
        }
        
        // Update existing row if it exists
        else
        {
            await _service.UpdateRowAsync(_equipmentSheetId, rowId, rowDict);
        }
    }

    private void OnCurrentCellValidating(CurrentCellValidatingEventArgs e)
    {
        
        var columnId = _columnMappingNameIdMap[e.Column.MappingName];

        var columnDataType = _columnIdDataTypeMap[columnId];
        
        var columnHeaderText = e.Column.HeaderText;

        var currentRow = e.RowData;
        
        _columnIdValidationRulesMap.TryGetValue(columnId, out var columnValidationRules);
        
        if (columnValidationRules == null)
        {
            e.IsValid = true;
            e.ErrorMessage = string.Empty; 
            return;
        }
        
        var allColumnValues = Rows
            .Where(row => row is IDictionary<string, object?> rowDict && rowDict.ContainsKey(e.Column.MappingName))
            .Select(row => (
                Value: ((IDictionary<string, object?>)row)[e.Column.MappingName]?.ToString() ?? string.Empty,
                Row: row
            ))
            .ToList();

        
        var validationResult = _cellValidator.ValidateCell(e.NewValue, currentRow, allColumnValues, columnDataType, columnHeaderText, columnValidationRules);
        if (!validationResult.IsValid)
        {
            e.ErrorMessage = validationResult.ErrorMessage;
            e.IsValid = false;
        }
        else
        {
            e.IsValid = true;
            e.ErrorMessage = string.Empty; 
        }
    }
    
    #endregion
    
    #region ToolBar commands realization
    
    private void OnExportToExcel()
    {
        ExcelExportManager.ExportToExcel(_dataGrid, "default", _notificationManager);
    }

    private void OnExportToPdf()
    {
        PdfExportManager.ExportToPdf(_dataGrid, "default", _notificationManager);
    }

    private void OnPrint()
    {
        _dataGrid.PrintSettings.PrintManagerBase = new PrintManager(_dataGrid);
        _dataGrid.ShowPrintPreview();
    }
    
    #endregion

    #region Navigation

    public void OnNavigatedTo(NavigationContext navigationContext)
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
        _dataGrid.GetValidationHelper().SetCurrentRowValidated(false);
    }

    public void FocusChanged(bool focusStatus)
    {
        if (focusStatus)
        {
            
        }
        else
        {
            _dataGrid.GetValidationHelper().SetCurrentRowValidated(false);
        }
    }

    #endregion

    #region Data Loading
    
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            var columns = await GetColumnsAsync();
            var rows = await GetRowsAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _dataGrid.Columns.Suspend();

                _dataGrid.Columns.Clear();
                foreach (var col in columns)
                {
                    _dataGrid.Columns.Add(col);
                }

                Rows.Clear();
                foreach (var row in rows)
                {
                    Rows.Add(row);
                }

                _dataGrid.Columns.Resume();
                _dataGrid.RefreshColumns();
                _dataGrid.View.Refresh();
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

    private async Task<ObservableCollection<ExpandoObject>> GetRowsAsync()
    {
        var rowModels = await _service.GetActiveRowsByEquipmentSheetIdAsync(_equipmentSheetId);
        var rowViewModels = new ObservableCollection<RowViewModel>();
        
        var rows = new ObservableCollection<ExpandoObject>();
        foreach (var row in rowModels.OrderBy(r => r.Position))
        {
            var rowViewModel = new RowViewModel(row);
            IDictionary<string, object?> rowDict = new ExpandoObject();
            
            foreach (var cell in row.Cells)
            {
                rowDict[cell.ColumnMappingName] = cell.Value;
            }
            rows.Add((ExpandoObject)rowDict);

            var meta = new RowMeta(
                RowId: row.Id,
                CellIds: row.Cells.ToDictionary(c => c.ColumnMappingName, c => c.Id)
            );
            _rowMetaMap[(ExpandoObject)rowDict] = meta;
        }

        return rows;
    }

    private async Task<Columns> GetColumnsAsync()
    {
        var columnModels = await _service.GetActiveColumnsByEquipmentSheetIdAsync(_equipmentSheetId);
        
        var frozenColumns = columnModels
            .Where(x => x.IsFrozen)
            .OrderBy(x => x.Order)
            .ToList();
        
        var nonFrozenColumns = columnModels
            .Where(x => !x.IsFrozen)
            .OrderBy(x => x.Order)
            .ToList();
        
        var sorted = frozenColumns.Concat(nonFrozenColumns);
        
        FrozenColumnCount = frozenColumns.Count;
        
        var columns = new Columns();
        foreach (var model in sorted)
        {
            _columnIdModelMap.Add(model.Id, model);
            _columnIdDataTypeMap.Add(model.Id, model.DataType);
            _columnIdHeaderTextMap.Add(model.Id, model.HeaderText);
            _columnIdValidationRulesMap.Add(model.Id, model.ValidationRules);
            _columnIdSpecificSettingsMap.Add(model.Id, model.SpecificSettings);
            _columnMappingNameIdMap.Add(model.MappingName, model.Id);
            
            var column = _columnFactory.CreateColumn(model, _gridHeaderBasedStyle);
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

    private void OnEditColumn(GridColumnContextMenuInfo args)
    {
        var editingColumnId = _columnMappingNameIdMap[args.Column.MappingName];
        var editingColumnModel = _columnIdModelMap[editingColumnId];
        
        var parameters = new NavigationParameters
        {
            { NavigationConstants.ScopedRegionManager, _scopedRegionManager },
            { "BaseHeaderStyle", _gridHeaderBasedStyle },
            { "EditingColumnModel", editingColumnModel },
            { "ColumnEditingCallback", new Action<ColumnEditingResult>(OnEditingColumnCallback) }
        };
            
        _scopedRegionManager.RequestNavigate(EquipmentSheetConstants.ColumnCreatorRegion, ViewNamesConstants.ColumnCreatorView, parameters);
    }

    private async void OnEditingColumnCallback(ColumnEditingResult result)
    {
        _scopedRegionManager?.Regions[EquipmentSheetConstants.ColumnCreatorRegion].RemoveAll();
        
        if (result.IsSuccessful != true)
        {
            return;
        }
        var editedColumnModel = result.EditedColumn;
        var editedColumnId = editedColumnModel.Id;

        try
        {
            await _service.UpdateColumnAsync(_equipmentSheetId, editedColumnId, editedColumnModel);
            
            await RefreshColumns();
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка оновлення характеристики", NotificationType.Error);
            _logger.LogError(e, "Failed to update column");
            throw;
        }
    }

    private async Task RefreshColumns()
    {
        _dataGrid.Columns.Suspend();
        _dataGrid.Columns.Clear();
        
        _columnMappingNameIdMap.Clear();
        _columnIdValidationRulesMap.Clear();
        _columnIdSpecificSettingsMap.Clear();
        _columnIdHeaderTextMap.Clear();
        _columnIdDataTypeMap.Clear();
        _columnIdModelMap.Clear();

        var columns = await GetColumnsAsync();
        
        _dataGrid.Columns.Suspend();
        
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            foreach (var col in columns)
            {
                _dataGrid.Columns.Add(col);
            }
        });
        
        _dataGrid.Columns.Resume();
        _dataGrid.RefreshColumns();
    }

    private async void OnRemoveColumn(GridColumnContextMenuInfo args)
    {
        try
        {
            var columnHeaderText = args.Column.HeaderText;
            
            var agreementTitle = "Видалення";
            var agreementMessage = $"Ви впевнені що хочете видалити характеристику '{columnHeaderText}' ? \n" +
                                   $"Буде видалено всі комірки для цієї характеристики";

            IsOverlayVisible = true;
            var confirm = await _dialogService.ShowDeleteConfirmationAsync(agreementTitle, agreementMessage);
            IsOverlayVisible = false;

            if (!confirm) return;
            
            var columnId = _columnMappingNameIdMap[args.Column.MappingName];
            
            // Soft remove column
            await _service.SoftRemoveColumnAsync(_equipmentSheetId, columnId);
            
            // Soft remove all cells associated with this column
            await _service.SoftRemoveCellsByColumnIdAsync(_equipmentSheetId, columnId);

            _columnIdModelMap.Remove(columnId);
            _columnIdDataTypeMap.Remove(columnId);
            _columnIdHeaderTextMap.Remove(columnId);
            _columnMappingNameIdMap.Remove(args.Column.MappingName);
            _columnIdSpecificSettingsMap.Remove(columnId);
            _columnIdValidationRulesMap.Remove(columnId);
            
            _dataGrid.Columns.Remove(args.Column);

            if (!_dataGrid.Columns.Any())
            {
                Rows.Clear();
                _rowMetaMap.Clear();
                
                SelectedItems.Clear();
                RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            }
            
            RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
            _notificationManager.Show($"Успішно видалено характеристику '{columnHeaderText}' та всі її комірки", NotificationType.Success);
            
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove column");
            _notificationManager.Show($"Помилка видалення характеристики '{args.Column.HeaderText}'", NotificationType.Error);
        }
    }

    private async void CreateColumn(ColumnCreationResult result)
    {
        _scopedRegionManager?.Regions[EquipmentSheetConstants.ColumnCreatorRegion].RemoveAll();
        if (!result.IsSuccessful) return;

        try
        {
            IsLoading = true;

            result.ColumnModel.Order = Columns.Count + 1;
            result.ColumnModel.Id = Guid.NewGuid();
            
            _columnIdModelMap.Add(result.ColumnModel.Id, result.ColumnModel);
            _columnIdDataTypeMap.Add(result.ColumnModel.Id, result.ColumnModel.DataType);
            _columnIdHeaderTextMap.Add(result.ColumnModel.Id, result.ColumnModel.HeaderText);
            _columnMappingNameIdMap.Add(result.ColumnModel.MappingName, result.ColumnModel.Id);
            _columnIdSpecificSettingsMap.Add(result.ColumnModel.Id, result.ColumnModel.SpecificSettings);
            _columnIdValidationRulesMap.Add(result.ColumnModel.Id, result.ColumnModel.ValidationRules);
            
            // Adding empty cells for new column
            var emptyCellsIds = new List<Guid>();
            foreach (var row in Rows)
            {
                var newId = Guid.NewGuid();
                emptyCellsIds.Add(newId);
                
                var rowMeta = _rowMetaMap[row];
                rowMeta.CellIds.Add(result.ColumnModel.MappingName, newId);
            }
            
            
            await _service.InsertColumnAsync(_equipmentSheetId, result.ColumnModel, emptyCellsIds);
            
            
            _dataGrid.Columns.Add(_columnFactory.CreateColumn(result.ColumnModel, _gridHeaderBasedStyle));
            
            _notificationManager.Show("Успішно створено характеристику", NotificationType.Success);
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

    private void OnRowDragStart(object? sender, GridRowDragStartEventArgs args)
    {
        var draggingExpandoRows = args.DraggingRecords
            .OfType<ExpandoObject>()
            .ToList();

        foreach (var rowId in draggingExpandoRows.Select(row => _rowMetaMap[row].RowId))
        {
            
        }
    }

    private void OnRowDragLeave(object? sender, GridRowDragLeaveEventArgs args)
    {
        Console.WriteLine("Drop position: " + args.DropPosition);
    }

    #region Private Methods

    private void GetNavigationParameters(INavigationParameters parameters)
    {
        var tabScopedServiceProvider = parameters.GetValue<IScopedProvider>("TabScopedServiceProvider");
        _service = tabScopedServiceProvider.Resolve<IEquipmentSheetService>();
        _scopedRegionManager ??= parameters[NavigationConstants.ScopedRegionManager] as IRegionManager;
        _scopedEventAggregator ??= parameters[NavigationConstants.ScopedEventAggregator] as IEventAggregator;
        _equipmentSheetId = (Guid)(parameters[EquipmentSheetConstants.EquipmentSheetId] ?? Guid.Empty);
    }

    private void SubscribeToRowsDragDropEvents()
    {
        _dataGrid.RowDragDropController.DragStart += OnRowDragStart;
        _dataGrid.RowDragDropController.DragLeave += OnRowDragLeave;
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

            Rows.Clear();
            Columns.Clear();
            await _service.DisposeAsync();

            _dataGrid = null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy data");
        }
    }
}
