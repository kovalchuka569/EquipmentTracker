using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Prism.Commands;
using Prism.Dialogs;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Notification.Wpf;
using Common.Logging;
using Core.Common.Helpers;
using Core.Common.RegionHelpers;
using Core.Interfaces;
using Core.Models;
using JetBrains.Annotations;
using Models.Common.Table.ColumnProperties;
using Models.Constants;
using Models.Dialogs;
using Models.Equipment;
using Models.Services;
using Presentation.Enums;
using Presentation.Interfaces;
using Presentation.Models;
using Presentation.UIManagers;
using Presentation.ViewModels.Common.Table;

namespace Presentation.ViewModels;

public class EquipmentSheetViewModel : BindableBase, INavigationAware, IDestructible, IRegionMemberLifetime,
    IDialogHost, IOverlayHost
{
    private readonly IAppLogger<EquipmentSheetViewModel> _logger;
    private readonly NotificationManager _notificationManager;
    private IEquipmentSheetService _service;
    private IExcelImportService _excelImportService;
    private IDialogManager _dialogManager;
    private IOverlayManager _overlayManager;
    private IExcelExportManager _excelExportManager;
    private IPdfExportManager _pdfExportManager;
    private ISyncfusionGridColumnManager _columnManager;
    private ICellValidatorService _cellValidatorService;
    private IRowValidatorService _rowValidatorService;
    private IRegionManager? _scopedRegionManager;
    private IEventAggregator? _scopedEventAggregator;
    private Guid _equipmentSheetId;
    private bool _isInitialized;

    public EquipmentSheetViewModel(
        IAppLogger<EquipmentSheetViewModel> logger,
        NotificationManager notificationManager,
        ISyncfusionGridColumnManager columnManager,
        IDialogManager dialogManager,
        IOverlayManager overlayManager,
        IExcelExportManager excelExportManager,
        IPdfExportManager pdfExportManager,
        ICellValidatorService cellValidatorService,
        IRowValidatorService rowValidatorService,
        IEquipmentSheetService service,
        IExcelImportService excelImportService)
    {
        _logger = logger;
        _notificationManager = notificationManager;
        _columnManager = columnManager;
        _dialogManager = dialogManager;
        _overlayManager = overlayManager;
        _excelExportManager = excelExportManager;
        _pdfExportManager = pdfExportManager;
        _cellValidatorService = cellValidatorService;
        _rowValidatorService = rowValidatorService;
        _service = service;
        _excelImportService = excelImportService;

        InitializeCommands();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    #region Properties

    private ObservableCollection<ItemViewModel> _items = new();

    public ObservableCollection<ItemViewModel> Items
    {
        get => _items;
        set
        {
            if (!SetProperty(ref _items, value))
                return;
            
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
        }
    }

    // TODO: Make the class of type ColumnInfo to store all these dictionaries (optional)

    private readonly Dictionary<Guid, BaseColumnProperties> _columnIdDomainMap = new();
    private readonly Dictionary<Guid, string> _columnIdHeaderTextMap = new();
    private readonly Dictionary<Guid, ColumnDataType> _columnIdDataTypeMap = new();
    private readonly Dictionary<string, Guid> _columnMappingNameIdMap = new();

    private Columns _columns = new();

    public Columns Columns
    {
        get => _columns;
        set
        {
            if (!SetProperty(ref _columns, value))
                return;
            
            RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
        }
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
        set => SetProperty(ref _isLoading, value);
    }


    public bool KeepAlive => true;

    private Style _gridHeaderBasedStyle = new();

    private SfDataGrid _dataGrid = new();

    public bool ColumnsEmptyTipVisibility => !IsLoading && !Columns.Any();
    public bool RowsEmptyTipVisibility => !IsLoading && Columns.Any() && !Items.Any();
    public bool MarkForDeleteContextMenuItemVisibility => GetMarkForDeleteContextMenuItemVisibility();
    public bool UnmarkForDeleteContextMenuItemVisibility => !MarkForDeleteContextMenuItemVisibility;

    #endregion

    #region Commands

    public DelegateCommand<SfDataGrid> DataGridLoadedCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand ExportToExcelCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand ExportToPdfCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand PrintCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand ExcelImportCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand<RowValidatingEventArgs> RowValidatingCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand
    {
        [UsedImplicitly]
        get; 
        private set;
    } = null!;

    public DelegateCommand<CurrentCellValidatingEventArgs> CurrentCellValidatingCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<CurrentCellBeginEditEventArgs> CurrentCellBeginEditCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<CurrentCellValueChangedEventArgs> CurrentCellValueChangedCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<AddNewRowInitiatingEventArgs> AddNewRowInitiatingCommand
    {
        [UsedImplicitly]
        get;
        private set;
    } = null!;

    public DelegateCommand<GridFilterItemsPopulatedEventArgs> FilterItemsPopulatedCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand MarkRowForDeleteCommand
    {
        [UsedImplicitly]get; 
        private set;
    } = null!;
    
    public DelegateCommand UnmarkRowForDeleteCommand
    {
        [UsedImplicitly]get; 
        private set;
    } = null!;

    public DelegateCommand<RecordDeletingEventArgs> KeyRemoveRowCommand
    {
        [UsedImplicitly]get; 
        private set;
    } = null!;

    public DelegateCommand<GridSelectionChangedEventArgs> SelectionChangedCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public DelegateCommand<QueryColumnDraggingEventArgs> QueryColumnDraggingCommand
    {
        [UsedImplicitly] 
        get;
        private set;
    } = null!;

    public DelegateCommand<ResizingColumnsEventArgs> ResizingColumnCommand
    {
        [UsedImplicitly]
        get; 
        private set;
    } = null!;

    public DelegateCommand OpenColumnsDesignerCommand
    {
        [UsedImplicitly] 
        get; 
        private set;
    } = null!;

    public AsyncDelegateCommand RefreshDataGridCommand
    {
        [UsedImplicitly] get;
        private set;
    } = null!;

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
        CurrentCellBeginEditCommand = new DelegateCommand<CurrentCellBeginEditEventArgs>(OnCurrentCellBeginEdit);
        CurrentCellValueChangedCommand = new DelegateCommand<CurrentCellValueChangedEventArgs>(OnCurrentCellValueChanged);
        AddNewRowInitiatingCommand = new DelegateCommand<AddNewRowInitiatingEventArgs>(OnAddNewRowInitiating);
        FilterItemsPopulatedCommand = new DelegateCommand<GridFilterItemsPopulatedEventArgs>(OnGridFilterItemsPopulated);
        MarkRowForDeleteCommand = new DelegateCommand(OnMarkRowsForDelete);
        UnmarkRowForDeleteCommand = new DelegateCommand(OnUnmarkRowsForDelete);
        KeyRemoveRowCommand = new DelegateCommand<RecordDeletingEventArgs>(OnKeyRemoveRow);
        SelectionChangedCommand = new DelegateCommand<GridSelectionChangedEventArgs>(OnSelectionChanged);
        QueryColumnDraggingCommand = new DelegateCommand<QueryColumnDraggingEventArgs>(OnQueryColumnDragging);
        ResizingColumnCommand = new DelegateCommand<ResizingColumnsEventArgs>(OnResizingColumn);
        OpenColumnsDesignerCommand = new DelegateCommand(OnOpenColumnsDesigner);
        RefreshDataGridCommand = new AsyncDelegateCommand(RefreshDataGrid);
    }

    #endregion

    private async Task RefreshDataGrid()
    {
        Items.Clear();
        Columns.Clear();
        _columnMappingNameIdMap.Clear();
        _columnIdDomainMap.Clear();
        _columnIdHeaderTextMap.Clear();
        _columnIdDataTypeMap.Clear();
        _columnIdHeaderTextMap.Clear();

        await LoadDataAsync();
    }

    private async void OnExcelImport()
    {
        _overlayManager.ShowOverlay(this);
        try
        {
            var result = await _dialogManager.ShowDialogAsync(DialogType.ExcelImportConfigurator, this);

            if (result is not { Result: ButtonResult.OK, Parameters: { } dialogParameters })
                return;

            var excelImportConfigurationResult =
                dialogParameters.GetValue<ExcelImportConfigurationResult>("ExcelImportConfigurationResult");

            var availableColumns = _columnIdDomainMap.Values
                .Select(c => (c.HeaderText, c.MappingName, c.ColumnDataType)).ToList();

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

            _columnIdDomainMap[updatedColumnId].HeaderWidth = width;

            await _service.UpdateColumnWidthAsync(_equipmentSheetId, updatedColumnId, width);
        }
        catch (Exception e)
        {
            args.Cancel = true;
            _notificationManager.Show("Помилка оновлення ширини", NotificationType.Error);
            _logger.LogError(e, "Error updating column width");
        }
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
                var columnViewModel = _columnIdDomainMap[columnId];
                columnViewModel.Order = i;
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
        RefreshMarkForDeleteContextMenuItemVisibility();
    }

    private void RefreshMarkForDeleteContextMenuItemVisibility()
    {
        RaisePropertyChanged(nameof(MarkForDeleteContextMenuItemVisibility));
        RaisePropertyChanged(nameof(UnmarkForDeleteContextMenuItemVisibility));
    }

    private bool GetMarkForDeleteContextMenuItemVisibility()
    {
        var first = SelectedItems.First();
        
        if(first is not ItemViewModel itemViewModel)
            return false;
        
        if(itemViewModel.RowViewModel.IsNew)
            return false;

        return !itemViewModel.RowViewModel.IsMarkedForDelete;
    }

    private async void OnKeyRemoveRow(RecordDeletingEventArgs args)
    {
        // Cancel standard delete operation
        args.Cancel = true;
        
        await UpdateMarkForDeleteInRows(true);
    }

    private async void OnMarkRowsForDelete()
    {
        await UpdateMarkForDeleteInRows(true);
    }

    private async void OnUnmarkRowsForDelete()
    {
       await UpdateMarkForDeleteInRows(false);
    }

    private async Task UpdateMarkForDeleteInRows(bool isMarkedForDelete)
    {
        var updatedRowIds = new List<Guid>();
        foreach (var item in SelectedItems)
        {
            if (item is not ItemViewModel itemViewModel)
                continue;
            
            itemViewModel.RowViewModel.IsMarkedForDelete = isMarkedForDelete;
            updatedRowIds.Add(itemViewModel.RowViewModel.Id);
        }
        
        await _service.UpdateRowsMarkedForDeleteAsync(_equipmentSheetId, updatedRowIds, isMarkedForDelete);
        
        RefreshMarkForDeleteContextMenuItemVisibility();
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
        if (e.Column.GetType() != typeof(GridCheckBoxColumn)) 
            return;

        if (!_isAddingNewRow)
        {
            
            // ONLY updating cell
            var item = e.Record as ItemViewModel ?? throw new InvalidOperationException("Item is not item view model");

            if (!item.RowViewModel.TryGetCellByMappingName(e.Column.MappingName, out var cell) || cell is null)
            {
                Console.WriteLine(e.Column.HeaderText);
                return;
            }

            if (!cell.IsNew)
            {
                Console.WriteLine("!cell.IsNew");
                var currentValue = cell.Value;

                if (currentValue is null) 
                    return;
                
                Console.WriteLine("current value is not null");

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
            var columnPropertiesViewModel = _columnIdDomainMap[columnId];

            var defaultValue = GetDefaultValue(columnPropertiesViewModel);

            var cellViewModel = new CellViewModel
            {
                ColumnMappingName = mappingName,
                Value = defaultValue,
            };

            itemViewModel.RowViewModel.AddCell(cellViewModel);
            itemViewModel[mappingName] = defaultValue;
        }
    }

    private object? GetDefaultValue(BaseColumnProperties domainProps)
    {
        return domainProps switch
        {
            BooleanColumnProperties booleanColumnProperties => booleanColumnProperties.DefaultValue,
            CurrencyColumnProperties currencyColumnProperties => currencyColumnProperties.DefaultValue,
            DateColumnProperties dateColumnProperties => dateColumnProperties.DefaultValue,
            LinkColumnProperties linkColumnProperties => linkColumnProperties.DefaultValue,
            ListColumnProperties listColumnProperties => listColumnProperties.DefaultValue,
            NumberColumnProperties numberColumnProperties => numberColumnProperties.DefaultValue,
            TextColumnProperties textColumnProperties => textColumnProperties.DefaultValue,
            _ => throw new ArgumentOutOfRangeException(nameof(domainProps))
        };
    }

    private void OnCurrentCellBeginEdit(CurrentCellBeginEditEventArgs args)
    {
        _isAddingNewRow = false;
    }

    private async void OnDataGridLoaded(SfDataGrid dataGrid)
    {
        _dataGrid = dataGrid;
        _gridHeaderBasedStyle = _dataGrid.TryFindResource("BasedGridHeaderStyle") as Style ??
                                throw new KeyNotFoundException();

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
                ColumnIdPropertiesMap = _columnIdDomainMap,
                Rows = Items
                    .Select(item => RowViewModel.ToDomain(item.RowViewModel))
                    .ToList()
            };

            rowValidateResult = _rowValidatorService.ValidateRow(rowValidationArgs);
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
                    if (row.RowViewModel.Id == rowId) // If its new row, skipping
                        continue;

                    row.RowViewModel.Position++; // Others move up by one position
                }

                await _service.InsertRowAsync(_equipmentSheetId,
                    RowViewModel.ToDomain(validatedItemViewModel.RowViewModel)); // Save new row in
            }
            catch (Exception ex)
            {
                _notificationManager.Show("Помилка створення запису", NotificationType.Error);
                _logger.LogError(ex, "Failed to creating row");
                throw;
            }
            finally
            {
                RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
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

        _columnIdDomainMap.TryGetValue(columnId, out var columnPropertiesDomain);
        if (columnPropertiesDomain is null)
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

        var validationResult = _cellValidatorService.ValidateCell(e.NewValue, currentRow, allColumnValues,
            columnDataType, columnHeaderText, columnPropertiesDomain);
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
        _excelExportManager.ExportToExcel(_dataGrid, "default", _notificationManager);
    }

    private void OnExportToPdf()
    {
        _pdfExportManager.ExportToPdf(_dataGrid, "default", _notificationManager);
    }

    private void OnPrint()
    {
        _dataGrid.PrintSettings.PrintManagerBase = new SyncfusionGridPrintManager(_dataGrid);
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
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));

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
            catch
            {
                /* ignored */
            }
        }
        finally
        {
            RaisePropertyChanged(nameof(RowsEmptyTipVisibility));
            RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
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
        // var columnModels = await _service.GetActiveColumnsByEquipmentSheetIdAsync(_equipmentSheetId);

        var columnModels = await _service.GetColumnPropsByEquipmentSheetIdAsync(_equipmentSheetId);

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
        foreach (var domain in sorted)
        {
            _columnIdDomainMap.Add(domain.Id, domain);
            _columnIdDataTypeMap.Add(domain.Id, domain.ColumnDataType);
            _columnIdHeaderTextMap.Add(domain.Id, domain.HeaderText);
            _columnMappingNameIdMap.Add(domain.MappingName, domain.Id);

            var column = _columnManager.CreateColumn(domain, _gridHeaderBasedStyle);
            columns.Add(column);
        }

        return columns;
    }

    #endregion

    #region Column Management

    private async void OnOpenColumnsDesigner()
    {
        var columnProperties = _columnIdDomainMap
            .Select(d => d.Value)
            .ToList();

        var dialogParams = new DialogParameters
        {
            { "ColumnProperties", columnProperties }
        };

        _overlayManager.ShowOverlay(this);
        var result = await _dialogManager.ShowDialogAsync(DialogType.ColumnDesigner, this, dialogParams);
        _overlayManager.HideOverlay(this);

        if (result.Result is ButtonResult.Cancel)
            return;

        var columnEditingResult = result.Parameters.GetValue<ColumnEditResult>("ColumnEditResult");

        if (columnEditingResult.NewColumns.Count > 0)
            await _service.AddColumnPropsAsync(_equipmentSheetId, columnEditingResult.NewColumns, []);

        if (columnEditingResult.EditedColumns.Count > 0)
            await _service.UpdateColumnPropsAsync(_equipmentSheetId, columnEditingResult.EditedColumns);

        var markedForDeleteList = columnEditingResult.EditedColumns
            .Where(c => c.MarkedForDelete)
            .Select(c => c.Id)
            .ToList();

        if (markedForDeleteList.Count > 0)
        {
            await _service.UpdateColumnsMarkedForDeleteAsync(_equipmentSheetId, markedForDeleteList, true);
        }
        
        var notMarkedForDeleteList = columnEditingResult.EditedColumns
            .Where(c => !c.MarkedForDelete)
            .Select(c => c.Id)
            .ToList();
        
        if (notMarkedForDeleteList.Count > 0)
        {
            await _service.UpdateColumnsMarkedForDeleteAsync(_equipmentSheetId, notMarkedForDeleteList, false);
        }

        await RefreshColumns();
    }

    private async Task RefreshColumns()
    {
        _dataGrid.Columns.Suspend();
        _dataGrid.Columns.Clear();

        _columnMappingNameIdMap.Clear();
        _columnIdDomainMap.Clear();
        _columnIdHeaderTextMap.Clear();
        _columnIdDataTypeMap.Clear();

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

        RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
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
                Items.Move(oldIndex,
                    targetIndex); // Move the row to the new position if the old index is less than the target index and not 0

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
        _scopedRegionManager ??= parameters["ScopedRegionManager"] as IRegionManager;
        _scopedEventAggregator ??= parameters["ScopedEventAggregator"] as IEventAggregator;
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

        IsRegionOverlayVisible =
            _scopedRegionManager.Regions[EquipmentSheetConstants.ColumnCreatorRegion].ActiveViews.Any();
    }

    #endregion

    public void Destroy()
    {
        try
        {
            if (_scopedRegionManager != null)
            {
                RegionCleanupHelper.CleanRegion(_scopedRegionManager, EquipmentSheetConstants.ColumnCreatorRegion);
            }

            Items.Clear();
            Columns.Clear();

            _dataGrid = new SfDataGrid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to destroy data");
        }
    }
}