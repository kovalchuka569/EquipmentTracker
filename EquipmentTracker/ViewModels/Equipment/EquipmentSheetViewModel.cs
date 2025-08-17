using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Input;
using Common.Logging;
using Core.Common.EquipmentSheetValidation.CellValidator;
using Core.Common.EquipmentSheetValidation.RowValidator;
using Core.Common.RegionHelpers;
using Core.Contracts;
using Core.Services.EquipmentDataGrid;
using Core.Services.Excel;
using EquipmentTracker.Common;
using EquipmentTracker.Common.DataGridExport;
using EquipmentTracker.Common.DialogManager;
using EquipmentTracker.Common.OverlayManager;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Factories.Interfaces;
using EquipmentTracker.ViewModels.Common.Table;
using Models.Common.Table;
using Models.Common.Table.ColumnSpecificSettings;
using Models.Common.Table.ColumnValidationRules;
using Models.Constants;
using Models.Dialogs;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Models.Services;
using Notification.Wpf;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Core.Common.Enums;
using Core.Models;

namespace EquipmentTracker.ViewModels.Equipment;

public class EquipmentSheetViewModel : BindableBase, INavigationAware, IDestructible, IRegionMemberLifetime, IDialogHost, IOverlayHost
{
    private readonly IAppLogger<EquipmentSheetViewModel> _logger;
    private readonly NotificationManager _notificationManager;
    private IContainerProvider _containerProvider;
    private IEquipmentSheetService _service;
    private IExcelImportService _excelImportService;
    private IDialogManager _dialogManager;
    private IOverlayManager _overlayManager;
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
        IContainerProvider containerProvider,
        IGridColumnFactory columnFactory,
        IDialogManager dialogManager,
        IOverlayManager overlayManager)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _containerProvider = containerProvider;
        _columnFactory = columnFactory;
        _dialogManager = dialogManager;
        _overlayManager = overlayManager;

        InitializeCommands();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    #region Properties

    private ObservableCollection<ItemViewModel> _items = new();
    public ObservableCollection<ItemViewModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

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
    
    private object? _dialogContent;
    public object? DialogContent
    {
        get => _dialogContent;
        set => SetProperty(ref _dialogContent, value);
    }
    
    private object? _overlayContent;
    public object? OverlayContent
    {
        get => _overlayContent;
        set => SetProperty(ref _overlayContent, value);
    }

    private bool _isOverlayOpen;
    public bool IsOverlayOpen
    {
        get => _isOverlayOpen;
        set => SetProperty(ref _isOverlayOpen, value);
    }
    
    private bool _isDialogOpen;
    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetProperty(ref _isDialogOpen, value);
    }

    private bool _isRegionOverlayVisible;
    public bool IsRegionOverlayVisible
    {
        get => _isRegionOverlayVisible;
        set => SetProperty(ref _isRegionOverlayVisible, value);
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
    public bool RowsEmptyTipVisibility => !IsLoading && Columns.Any() && !Items.Any();
    public bool DeleteRowContextMenuItemVisibility => SelectedItems.Any();

    #endregion

    #region Commands
    
    public DelegateCommand<SfDataGrid> DataGridLoadedCommand { get; private set; }
    
    public DelegateCommand ExportToExcelCommand { get; private set; }
    
    public DelegateCommand ExportToPdfCommand { get; private set; }
    
    public DelegateCommand PrintCommand { get; private set; }
    
    public DelegateCommand ExcelImportCommand { get; private set; }
    
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
    
    public DelegateCommand<ResizingColumnsEventArgs> ResizingColumnCommand { get; private set; }

    private void InitializeCommands()
    {
        DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
        
        ExportToExcelCommand = new DelegateCommand(OnExportToExcel);
        ExportToPdfCommand = new DelegateCommand(OnExportToPdf);
        PrintCommand = new DelegateCommand(OnPrint);
        ExcelImportCommand = new DelegateCommand(OnExcelImport);

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
        ResizingColumnCommand = new DelegateCommand<ResizingColumnsEventArgs>(OnResizingColumn);
    }

    #endregion

    private async void OnExcelImport()
    { 
        _overlayManager.ShowOverlay(this);
        try
        {
            var result = await _dialogManager.ShowDialogAsync(DialogType.ExcelImportConfigurator, this);

            if (result is not { Result: ButtonResult.OK, Parameters: { } dialogParameters })
                return;
        
            var excelImportConfigurationResult = dialogParameters.GetValue<ExcelImportConfigurationResult>("ExcelImportConfigurationResult");

            var availableColumns = _columnIdModelMap.Values.Select(c => (c.HeaderText, c.MappingName, c.DataType)).ToList();

            var excelImportConfig = new ExcelImportConfig
            {
                FilePath = excelImportConfigurationResult.SelectedFilePath,
                SheetName = excelImportConfigurationResult.SelectedSheetName,
                HeadersRange = excelImportConfigurationResult.HeadersRange,
                RowRangeStart = excelImportConfigurationResult.RowRangeStart,
                RowRangeEnd = excelImportConfigurationResult.RowRangeEnd,
                AvailableColumns = availableColumns
            };

            var importedRows = await _excelImportService.ImportRowsAsync(excelImportConfig);

            // Pushing existing rows depending on how many were imported
            foreach (var item in Items)
            {
                item.RowViewModel.Position += importedRows.Count;
            }

            // Reverse the order of imported rows to match the original order
            importedRows.Reverse();
            
            // Insert new items at the beginning of the items collection
            var pos = 1;
            foreach (var row in importedRows)
            {
                row.Position = pos++;
                var itemViewModel = new ItemViewModel(RowViewModel.FromDomain(row));
                Items.Insert(0, itemViewModel);
            }

            await _service.InsertRowsAsync(_equipmentSheetId, importedRows);
        }
        finally
        {
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            _overlayManager.HideOverlay(this);
        }
    }

    private async void OnResizingColumn(ResizingColumnsEventArgs args)
    {
        try
        {
            if (args.Reason != ColumnResizingReason.Resized) return;
            
            var actualIndex = args.ColumnIndex - 1;
            var width = args.Width;
            var mappingName = _dataGrid.Columns[actualIndex].MappingName;
            var updatedColumnId = _columnMappingNameIdMap[mappingName];
            
            _columnIdModelMap[updatedColumnId].Width = width;

            await _service.UpdateColumnWidthAsync(_equipmentSheetId, updatedColumnId, width);
        }
        catch (Exception e)
        {
            args.Cancel = true;
            _notificationManager.Show("Помилка оновлення ширини", NotificationType.Error);
            _logger.LogError(e, "Error updating column width");
        }
    }
    

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
            var confirm = await ProcessRemovalItem();
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
            await ProcessRemovalItem();
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка видалення записів", NotificationType.Error); 
            _logger.LogError(e, "Error removing rows");
        }
    }
    
    private async Task<bool> ProcessRemovalItem()
    {
        using var cts = new CancellationTokenSource();

        var itemsToRemove = SelectedItems.Cast<ItemViewModel>().ToList();

        var removeItemsCount = itemsToRemove.Count;
        if (removeItemsCount == 0) return false;
        
        var deletedRecordCountText = PluralizedHelper.GetPluralizedText(removeItemsCount, "запис", "записи", "записів");

        var confirm = await RemoveRowAgreement(removeItemsCount, deletedRecordCountText);
        if (!confirm) return false;

        return await ExecuteRemovalRows(itemsToRemove, deletedRecordCountText);
    }
    
    private async Task<bool> ExecuteRemovalRows(List<ItemViewModel> itemsToRemove, string deletedRecordCountText)
    {
        var rowIdsToRemove = itemsToRemove
            .Select(r => r.RowViewModel.Id)
            .ToList();
        
        try
        {
            await _service.SoftRemoveRowsAsync(_equipmentSheetId, rowIdsToRemove);

            foreach (var item in itemsToRemove)
            {
                Items.Remove(item);
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

        var message = removeItemsCount == 1
            ? "Ви впевнені що хочете видалити цей запис?\nБуде видалено всі комірки для цього запису"
            : $"Ви впевнені що хочете видалити {deletedRecordCountText}?\nБуде видалено всі комірки для цих записів";
        
        var title = removeItemsCount == 1
            ? "Видалити вибраний запис?"
            : "Видалити вибрані записи?";
        
        var parameters = new DialogParameters
        {
            {"DialogBoxParameters", new DialogBoxParameters
            {
                Title = title,
                Message = message,
                Icon = DialogBoxIcon.Trash,
                Buttons = DialogBoxButtons.DeleteCancel,
                ButtonsText = ["Видалити", "Відмінити"]
            }}
        };
        
        _overlayManager.ShowOverlay(this);
        
        await _dialogManager.ShowDialogAsync(DialogType.DialogBox, this, parameters);
        
        _overlayManager.HideOverlay(this);

        return false;
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
            var item =  e.Record as ItemViewModel ?? throw new InvalidOperationException("Item is not item view model");
            
            if(!item.RowViewModel.TryGetCellByMappingName(e.Column.MappingName, out var cell) || cell is null) return;
            
            if (!cell.IsNew)
            {
                var currentValue = cell.Value;
                
                if(currentValue is null) return;
                   
                await _service.UpdateCellValueAsync(_equipmentSheetId, cell.Id, currentValue);
            }
               
        }
            
        // Make not valid for activate row validation
        _dataGrid.GetValidationHelper().SetCurrentRowValidated(false);
    }

    private bool _isAddingNewRow;
    private void OnAddNewRowInitiating(AddNewRowInitiatingEventArgs args)
    {
        _isAddingNewRow = true;
        if (args.NewObject is not ItemViewModel itemViewModel) return;
        
        foreach (var entry in _columnMappingNameIdMap)
        {
            var mappingName = entry.Key;
            var columnId = entry.Value;
            var dataType = _columnIdDataTypeMap[columnId];
                
            var defaultValue = GetDefaultValue(dataType);
            
            if (_columnIdSpecificSettingsMap.TryGetValue(columnId, out var columnSpecificSettings) &&
                columnSpecificSettings is CheckBoxColumnSpecificSettings checkBoxSettings)
            {
                defaultValue = checkBoxSettings.DefaultValue;
            }

            var cellViewModel = new CellViewModel
            {
                ColumnMappingName = mappingName,
                Value = defaultValue,
            };

            itemViewModel.RowViewModel.AddCell(cellViewModel);
            itemViewModel[mappingName] = defaultValue;

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

        if (_isInitialized) return;
        
        await LoadDataAsync();
        _isInitialized = true;
    }

    #region Row & cell validation and save methods

    private void OnRowValidating(RowValidatingEventArgs e)
    {
        RowValidationResult rowValidateResult;
        
        try
        {
            if (e.RowData is not ItemViewModel currentItemViewModel) return;
            
            var rowValidationArgs = new RowValidationArgs
            {
                CurrentRow = RowViewModel.ToDomain(currentItemViewModel.RowViewModel),
                ColumnMappingNameIdMap = _columnMappingNameIdMap,
                ColumnIdHeaderTextMap = _columnIdHeaderTextMap,
                ColumnIdDataTypeMap = _columnIdDataTypeMap,
                ColumnIdValidationRulesMap = _columnIdValidationRulesMap,
                Rows = Items
                    .Select(item => RowViewModel.ToDomain(item.RowViewModel))
                    .ToList()
            };
            
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
        if (e.RowData is not ItemViewModel validatedItemViewModel) 
            return;
        
        var rowId = validatedItemViewModel.RowViewModel.Id;

        // Creating new row if it doesn't exist
        if (validatedItemViewModel.RowViewModel.IsNew)
        {
            try
            {
                validatedItemViewModel.RowViewModel.Position = 1; // New row should be inserted at the first position
                
                // Shift positions of existing rows down by one
                foreach (var row in Items)
                {
                    if (row.RowViewModel.Id == rowId)   // If its new row, skipping
                        continue;
                    
                    row.RowViewModel.Position++;    // Others move up by one position
                }
                
                await _service.InsertRowAsync(_equipmentSheetId, RowViewModel.ToDomain(validatedItemViewModel.RowViewModel));   // Save new row in
                
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Помилка стоврення запису", NotificationType.Error);
                _logger.LogError(ex, "Failed to creating row");
                throw;
            }
            finally
            {
                validatedItemViewModel.RowViewModel.IsNew = false; // Making the row not new
            }
        }
        
        // Update row if it exists
        else
        {
            var updatedRowModel = RowViewModel.ToDomain(validatedItemViewModel.RowViewModel);
            await _service.UpdateRowAsync(_equipmentSheetId, rowId, updatedRowModel);
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

        var mappingName = e.Column.MappingName;

        var current = (ItemViewModel)e.RowData;
        var currentId = current.RowViewModel.Id;
        
        var allColumnValues = Items
            .Where(item => item.RowViewModel.Id != currentId)       
            .Select(item => item[mappingName]?.ToString()?.Trim())  
            .Where(s => !string.IsNullOrEmpty(s))                   
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

    #endregion

    #region Data Loading
    
    private async Task LoadDataAsync()
    {
        try
        {
            IsLoading = true;
            
            var columns = await GetColumnsAsync();
            var items = await GetRowsAsync();
            
            await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                _dataGrid.Columns.Clear();
    
                foreach (var col in columns)
                {
                    _dataGrid.Columns.Add(col);
                }
    
                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
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

    private async Task<ObservableCollection<ItemViewModel>> GetRowsAsync()
    {
        var rowModels = await _service.GetActiveRowsByEquipmentSheetIdAsync(_equipmentSheetId);
        return new ObservableCollection<ItemViewModel>(
            rowModels
                .Select(rm => new ItemViewModel(RowViewModel.FromDomain(rm))));
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

            IsRegionOverlayVisible = true;
            var confirm = await _dialogManager.ShowDeleteConfirmationAsync(agreementTitle, agreementMessage);
            IsRegionOverlayVisible = false;

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
                Items.Clear();
                
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
            object? value = null;
            
            var mappingName = result.ColumnModel.MappingName;

            // If new column is checkbox - set default value
            if (result.ColumnModel.DataType is ColumnDataType.Boolean &&
                result.ColumnModel.SpecificSettings is CheckBoxColumnSpecificSettings checkBoxColumnSpecificSettings)
            {
                value = checkBoxColumnSpecificSettings.DefaultValue;
            }
            
            var newCells = new List<CellModel>();
            
            foreach (var itemViewModel in Items)
            {

                var newCellViewModel = new CellViewModel
                {
                    RowId = itemViewModel.RowViewModel.Id,
                    Value = value,
                    ColumnMappingName = mappingName
                };
                
                itemViewModel.RowViewModel.AddCell(newCellViewModel);
                
                newCells.Add(CellViewModel.ToDomain(newCellViewModel));
                
                itemViewModel[mappingName] = value;
                
                itemViewModel.RowViewModel.TrySetCellValueByMappingName(result.ColumnModel.MappingName, value);
            }
            
            await _service.InsertColumnAsync(_equipmentSheetId, result.ColumnModel, newCells);
            
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

    private async void OnRowsDropped(object? sender, GridRowDroppedEventArgs args)
    {
        if (args.IsFromOutSideSource)
            return; // Leave if dropped from outside source

        if (args.Data is not DataObject dataObject)
            return; // Check that the data is represented as a DataObject, otherwise leave

        if (!dataObject.GetDataPresent("Records")) 
            return; // Check that the DataObject has the "Records" format, otherwise leave

        if (dataObject.GetData("Records") is not IEnumerable recordsObj)
            return; // Get the "Records" format data and check that it is IEnumerable

        var droppedRows = new List<ItemViewModel>();

        // Convert the elements to ItemViewModel and collect them into a list
        foreach (var item in recordsObj)
        {
            if (item is ItemViewModel row)
                droppedRows.Add(row);
        }

        if (droppedRows.Count == 0)
            return; // If there is no string of the required type, exit the handler
        
        if (args.TargetRecord is not int targetIndex)
            return; // Leave if target record is not int index
        
        // Find the index of the insertion target in the items collection
        if (args.DropPosition == DropPosition.DropBelow)
            targetIndex++;
        
        // If the target index is out of bounds, adjust it
        if (targetIndex < 0)
            targetIndex = 0;
        
        if (targetIndex > Items.Count)
            targetIndex = Items.Count;
        
        // Move the dragged rows one by one, preserving their order
        foreach (var row in droppedRows)
        {
            var oldIndex = Items.IndexOf(row); // Get old index of the row in the items collection
            
            if (oldIndex < 0)
                continue; // Not valid, skip it

            if (oldIndex < targetIndex)
                targetIndex--; // Move the target index down if the old index is less than the target index
            
            if (oldIndex != targetIndex)
                Items.Move(oldIndex, targetIndex); // Move the row to the new position if the old index is less than the target index and not 0
            
            targetIndex++; // If the element is already in place, simply shift the targetIndex so that the next one is inserted after it
        }

        // Update view model positions according to the new order
        for (var i = 0; i < Items.Count; i++)
        {
            Items[i].RowViewModel.Position = i + 1;
        }

        try
        {
            var rowsModels = Items.Select(row => RowViewModel.ToDomain(row.RowViewModel)).ToList();
            await _service.UpdateRowsAsync(_equipmentSheetId, rowsModels, true);
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка оновлення позицій записів", NotificationType.Error);
            _logger.LogError(e, "Failed to update rows in sorting");
            throw;
        }
    }

    #region Private Methods

    private void GetNavigationParameters(INavigationParameters parameters)
    {
        var tabScopedServiceProvider = parameters.GetValue<IScopedProvider>("TabScopedServiceProvider");
        _service = tabScopedServiceProvider.Resolve<IEquipmentSheetService>();
        _excelImportService = tabScopedServiceProvider.Resolve<IExcelImportService>();
        
        _scopedRegionManager ??= parameters[NavigationConstants.ScopedRegionManager] as IRegionManager;
        _scopedEventAggregator ??= parameters[NavigationConstants.ScopedEventAggregator] as IEventAggregator;
        _equipmentSheetId = (Guid)(parameters[EquipmentSheetConstants.EquipmentSheetId] ?? Guid.Empty);
    }

    private void SubscribeToRowsDragDropEvents()
    {
        _dataGrid.RowDragDropController.Dropped += OnRowsDropped;
    }

    private void SubscribeToRegionsChanges()
    {
        if (_scopedRegionManager == null) return;

        _scopedRegionManager.Regions[EquipmentSheetConstants.ColumnCreatorRegion]
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

        IsRegionOverlayVisible = _scopedRegionManager.Regions[EquipmentSheetConstants.ColumnCreatorRegion].ActiveViews.Any();
    }

    #endregion

    public async void Destroy()
    {
        try
        {
            
            if (_scopedRegionManager != null)
            {
                RegionCleanupHelper.CleanRegion(_scopedRegionManager, EquipmentSheetConstants.ColumnCreatorRegion);
            }

            Items.Clear();
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
