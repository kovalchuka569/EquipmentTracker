using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows;
using System.Globalization;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;
using Common.Logging;
using Syncfusion.XlsIO;
using Core.Services.EquipmentDataGrid;
using EquipmentTracker.Common;
using EquipmentTracker.Common.DataGridExport;
using EquipmentTracker.Common.DialogManager;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Constants.Equipment;
using EquipmentTracker.ViewModels.Equipment.DataGrid.Import;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Models.Equipment.ColumnSpecificSettings;
using Notification.Wpf;
using Syncfusion.UI.Xaml.Grid;
using EquipmentTracker.ViewModels.Equipment.DataGrid.TemplateCreators;
using Models.Equipment.ColumnSettings;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Controls.Grid;
using Syncfusion.Windows.Shared;
using Columns = Syncfusion.UI.Xaml.Grid.Columns;
using CurrentCellValidatingEventArgs = Syncfusion.UI.Xaml.Grid.CurrentCellValidatingEventArgs;
using DelegateCommand = Prism.Commands.DelegateCommand;
using GridSelectionChangedEventArgs = Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs;
using SelectionChangedEventArgs = Syncfusion.UI.Xaml.Grid.SelectionChangedEventArgs;

namespace EquipmentTracker.ViewModels.Equipment.DataGrid;

public class EquipmentDataGridViewModel : BindableBase, INavigationAware, IDestructible, IRegionMemberLifetime
{
    private readonly IEquipmentSheetService _equipmentSheetService;
    private readonly NotificationManager _notificationManager;
    private readonly IAppLogger<EquipmentDataGridViewModel> _logger;
    private readonly IDialogManager _dialogManager;
    private IRegionManager _scopedRegionManager;
    private Guid _tableId;
    private string _equipmentSheetName;

    private readonly Dictionary<string, Guid> _columnMappingNameIdMap = new();
    private readonly Dictionary<GridColumnBase, ColumnItem> _columnItemMap = new();

    private Dictionary<ExpandoObject, Guid> _equipmentIds = new();
    
    private Columns _columns = new();
    private ObservableCollection<RowItem> _equipments = new();
    private ObservableCollection<object> _selectedItems = new();

    public ObservableCollection<RowItem> Equipments
    {
        get => _equipments;
        set => SetProperty(ref _equipments, value);
    }
    public Columns Columns
    {
        get => _columns;
        set => SetProperty(ref _columns, value);
    }

    public ObservableCollection<object> SelectedItems
    {
        get => _selectedItems;
        set => SetProperty(ref _selectedItems, value);
    }

    private int _frozenColumnCount;
    public int FrozenColumnCount
    {
        get => _frozenColumnCount;
        set => SetProperty(ref _frozenColumnCount, value);
    }

    private bool _progressBarVisibility;
    public bool ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set => SetProperty(ref _progressBarVisibility, value);
    }

    private int _importProgress;

    public int ImportProgress
    {
        get => _importProgress;
        set => SetProperty(ref _importProgress, value);
    }

    private bool _isImporting;

    public bool IsImporting
    {
        get => _isImporting;
        set
        {
            SetProperty(ref _isImporting, value);
            ProgressBarVisibility = value;
        }
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
                RaisePropertyChanged(nameof(DataEmptyTipVisibility));
            }
        }
    }

    private bool _isInitialized = false;


    public bool ColumnsEmptyTipVisibility => !IsLoading && !Columns.Any();
    public bool DataEmptyTipVisibility => !IsLoading && Columns.Any() && !Equipments.Any();
    public bool DeleteRowContextMenuItemVisibility => SelectedItems.Any();

    private SfDataGrid _sfDataGrid;
    private UserControl _userControl;
    
    private Style _baseGridHeaderStyle;
    private Style _baseGridCellStyle;

    private bool _isOverlayVisible;

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible ||
               _scopedRegionManager.Regions["ColumnCreatorRegion"].ActiveViews.Any() ||
               _scopedRegionManager.Regions["SheetSelectorRegion"].ActiveViews.Any();
        set => SetProperty(ref _isOverlayVisible, value);
    }

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (!_isInitialized)
        {
            if (navigationContext.Parameters[NavigationConstants.ScopedRegionManager] is IRegionManager scopedRegionManager)
            {
                _scopedRegionManager = scopedRegionManager;

                if (_scopedRegionManager.Regions.ContainsRegionWithName("ContentRegion"))
                {
                    var region = _scopedRegionManager.Regions["ContentRegion"].ActiveViews.FirstOrDefault() as UserControl;
                    if (region != null)
                    {
                        var baseHeaderStyle = region.FindResource("SyncfusionGridHeaderCellControlStyle") as Style;
                        var baseCellStyle = region.FindResource("SyncfusionGridCellStyle") as Style;
                        _baseGridHeaderStyle = baseHeaderStyle;
                        _baseGridCellStyle = baseCellStyle;
                    }
                }

                if (_scopedRegionManager.Regions.ContainsRegionWithName("ColumnCreatorRegion"))
                {
                    _scopedRegionManager.Regions["ColumnCreatorRegion"].ActiveViews.CollectionChanged +=
                        OnActiveViewsChanged;

                    RaisePropertyChanged(nameof(IsOverlayVisible));
                }

                if (_scopedRegionManager.Regions.ContainsRegionWithName("SheetSelectorRegion"))
                {
                    _scopedRegionManager.Regions["SheetSelectorRegion"].ActiveViews.CollectionChanged +=
                        OnActiveViewsChanged;
                }
            }
            
            if (navigationContext.Parameters["TableId"] is Guid tableId)
            {
                _tableId = tableId;
            }

            if (navigationContext.Parameters[EquipmentConstants.EquipmentSheetName] is string equipmentSheetName)
            {
                _equipmentSheetName = equipmentSheetName;
            }
        
            try
            {   
                ProgressBarVisibility = true;
                IsLoading = true;
                
                await LoadColumnsAsync();
                await LoadRowsAsync();

            }
            finally
            {
                IsLoading = false;
                ProgressBarVisibility = false;
                _isInitialized = true;
            }
        }
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public Prism.Commands.DelegateCommand<QueryColumnDraggingEventArgs> ColumnDraggingCommand { get; }
    public Prism.Commands.DelegateCommand<ResizingColumnsEventArgs> ResizingColumnCommand { get; }
    public DelegateCommand AddColumnCommand { get; }
    public Prism.Commands.DelegateCommand<SfDataGrid> EquipmentDataGridLoadedCommand { get; }
    public Prism.Commands.DelegateCommand<GridColumnContextMenuInfo> EditColumnCommand { get; }
    public Prism.Commands.DelegateCommand<RowValidatingEventArgs> RowValidatingCommand { get; }
    public Prism.Commands.DelegateCommand<RowValidatedEventArgs> RowValidatedCommand { get; }
    public Prism.Commands.DelegateCommand<CellTappedEventArgs> CellTappedCommand { get; }
    public Prism.Commands.DelegateCommand<AddNewRowInitiatingEventArgs> AddNewRowInitiatingCommand { get; }
    public Prism.Commands.DelegateCommand<UserControl> UserControlLoadedCommand { get; }
    public Prism.Commands.DelegateCommand<CurrentCellValidatingEventArgs> CurrentCellEndEditCommand { get; }
    public Prism.Commands.DelegateCommand<GridFilterItemsPopulatedEventArgs> FilterItemsPopulatedCommand { get; }

    public DelegateCommand ExportToExcelCommand { get; set; }
    public DelegateCommand ExportToPdfCommand { get; set; }
    public DelegateCommand PrintCommand { get; set; }
    public DelegateCommand ExcelImportCommand { get; set; }

    public Prism.Commands.DelegateCommand<GridColumnContextMenuInfo> RemoveColumnCommand { get; set; }
    public DelegateCommand RemoveRowCommand { get; set; }
    public Prism.Commands.DelegateCommand<RecordDeletingEventArgs> ButtonRemoveRowCommand { get; set; }

    public Prism.Commands.DelegateCommand<GridSelectionChangedEventArgs> SelectionChangedCommand { get; set; }

    public EquipmentDataGridViewModel(IEquipmentSheetService equipmentSheetService,
        NotificationManager notificationManager,
        IDialogManager dialogManager,
        IAppLogger<EquipmentDataGridViewModel> logger)
    {
        _equipmentSheetService = equipmentSheetService;
        _notificationManager = notificationManager;
        _dialogManager = dialogManager;
        _logger = logger;

        ColumnDraggingCommand = new Prism.Commands.DelegateCommand<QueryColumnDraggingEventArgs>(OnColumnDragging);
        ResizingColumnCommand = new Prism.Commands.DelegateCommand<ResizingColumnsEventArgs>(OnResizingColumn);
        AddColumnCommand = new DelegateCommand(OnAddColumn);
        EquipmentDataGridLoadedCommand = new Prism.Commands.DelegateCommand<SfDataGrid>(OnEquipmentDataGridLoaded);
        EditColumnCommand = new Prism.Commands.DelegateCommand<GridColumnContextMenuInfo>(OnEditColumn);
        RowValidatingCommand = new Prism.Commands.DelegateCommand<RowValidatingEventArgs>(OnRowValidating);
        RowValidatedCommand = new Prism.Commands.DelegateCommand<RowValidatedEventArgs>(OnRowValidated);
        CellTappedCommand = new Prism.Commands.DelegateCommand<CellTappedEventArgs>(OnCellTapped);
        AddNewRowInitiatingCommand =
            new Prism.Commands.DelegateCommand<AddNewRowInitiatingEventArgs>(OnAddNewRowInitiating);
        UserControlLoadedCommand = new Prism.Commands.DelegateCommand<UserControl>(OnUserControlLoaded);
        CurrentCellEndEditCommand =
            new Prism.Commands.DelegateCommand<CurrentCellValidatingEventArgs>(OnCurrentCellEndEdit);
        ExcelImportCommand = new DelegateCommand(async () => await OnImportFromExcelAsync());
        FilterItemsPopulatedCommand =
            new Prism.Commands.DelegateCommand<GridFilterItemsPopulatedEventArgs>(OnFilterItemsPopulated);

        ExportToExcelCommand = new DelegateCommand(OnExportToExcel);
        ExportToPdfCommand = new DelegateCommand(OnExportToPdf);
        PrintCommand = new DelegateCommand(OnPrint);

        RemoveColumnCommand = new Prism.Commands.DelegateCommand<GridColumnContextMenuInfo>(OnRemoveColumn);
        RemoveRowCommand = new DelegateCommand(OnRemoveRow);
        ButtonRemoveRowCommand = new Prism.Commands.DelegateCommand<RecordDeletingEventArgs>(OnButtonRemoveRow);
        SelectionChangedCommand = new Prism.Commands.DelegateCommand<GridSelectionChangedEventArgs>(OnSelectionChanged);

        SubscribeToEvents();
    }

    private void OnSelectionChanged(GridSelectionChangedEventArgs args)
    {
        RaisePropertyChanged(nameof(DeleteRowContextMenuItemVisibility));
    }

    private async void OnButtonRemoveRow(RecordDeletingEventArgs args)
    {
        bool confirm = await ProcessRemoval();
        args.Cancel = !confirm;
    }

    private async void OnRemoveRow()
    {
        await ProcessRemoval();
    }

    private async Task<bool> ProcessRemoval()
    {
        using var cts = new CancellationTokenSource();

        var itemsToRemove = SelectedItems.Cast<ExpandoObject>().ToList();
        if (itemsToRemove.Count == 0) return false;

        List<Guid> idsToRemove = itemsToRemove.Select(x => _equipmentIds[x]).ToList();
        int removeItemsCount = itemsToRemove.Count;
        string deletedRecordCountText =
            PluralizedHelper.GetPluralizedText(removeItemsCount, "запис", "записи", "записів");

        bool confirm = await RemoveRowAgreement(removeItemsCount, deletedRecordCountText);
        if (!confirm) return false;

        return await ExecuteRemoval(itemsToRemove, idsToRemove, deletedRecordCountText, cts);
    }

    private async Task<bool> ExecuteRemoval(List<ExpandoObject> itemsToRemove, List<Guid> idsToRemove,
        string deletedRecordCountText, CancellationTokenSource cts)
    {
        ProgressBarVisibility = true;
        try
        {
           // await _equipmentSheetService.RemoveItemsAsync(idsToRemove, cts.Token);

            foreach (var item in itemsToRemove)
            {
                cts.Token.ThrowIfCancellationRequested();
              //  Equipments.Remove(item);
            }

            SelectedItems.Clear();
            _notificationManager.Show($"Успішно видалено {deletedRecordCountText}", NotificationType.Success);
            return true;
        }
        catch (OperationCanceledException)
        {
            _notificationManager.Show("Операцію видалення скасовано", NotificationType.Warning);
            return false;
        }
        catch (Exception e)
        {
            _notificationManager.Show($"Помилка видалення {e.Message}", NotificationType.Error);
            _logger.LogError(e, "Error removing equipments");
            return false;
        }
        finally
        {
            ProgressBarVisibility = false;
        }
    }

    private async Task<bool> RemoveRowAgreement(int removeItemsCount, string deletedRecordCountText)
    {
        if (removeItemsCount == 0) return false;

        string message = removeItemsCount == 1
            ? $"Ви впевнені що хочете видалити цей запис? \nБуде видалено всі комірки для цього запису"
            : $"Ви впевнені що хочете видалити {deletedRecordCountText}? \nБуде видалено всі комірки для цих записів";

        IsOverlayVisible = true;
        try
        {
            return await _dialogManager.ShowDeleteConfirmationAsync("Видалення", message);
        }
        finally
        {
            IsOverlayVisible = false;
        }
    }


    private async void OnRemoveColumn(GridColumnContextMenuInfo contextInfo)
    {
        using var cts = new CancellationTokenSource();
        var columnItem = _columnItemMap[contextInfo.Column];
        string columnHeaderText = columnItem.Settings.HeaderText;
        Guid columnId = columnItem.Id;

        string title = "Видалення";
        string message = $"Ви впевнені що хочете видалити характеристику '{columnHeaderText}' ? \n" +
                         $"Буде видалено всі комірки для цієї характеристики";


        IsOverlayVisible = true;
        bool confirm = await _dialogManager.ShowDeleteConfirmationAsync(title, message);
        IsOverlayVisible = false;

        if (confirm)
        {
            ProgressBarVisibility = true;
            try
            {
               // await _equipmentSheetService.RemoveColumnAsync(columnId, cts.Token);
                Columns.Remove(contextInfo.Column);
                _notificationManager.Show($"Успішно видалено характеристику '{columnHeaderText}'",
                    NotificationType.Success);
            }
            catch (OperationCanceledException)
            {
                _notificationManager.Show("Операцію видалення скасовано", NotificationType.Warning);
                throw;
            }
            catch (Exception e)
            {
                _notificationManager.Show($"Помилка видалення {e}", NotificationType.Error);
                _logger.LogError(e, "Error removing column");
                throw;
            }
            finally
            {
                ProgressBarVisibility = false;
            }
        }
    }

    private void SubscribeToEvents()
    {
        ((INotifyCollectionChanged)Columns).CollectionChanged += Columns_CollectionChanged;
        ((INotifyCollectionChanged)Equipments).CollectionChanged += Equipments_CollectionChanged;
    }

    private void UnsubscribeFromEvents()
    {
        _sfDataGrid.RowDragDropController.Dropped -= RowDragDropControllerOnDropped;
        ((INotifyCollectionChanged)Columns).CollectionChanged -= Columns_CollectionChanged;
        ((INotifyCollectionChanged)Equipments).CollectionChanged -= Equipments_CollectionChanged;
    }

    private void Columns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(ColumnsEmptyTipVisibility));
        RaisePropertyChanged(nameof(DataEmptyTipVisibility));
    }


    private void Equipments_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(DataEmptyTipVisibility));
    }

    private void OnPrint()
    {
        _sfDataGrid.PrintSettings.PrintManagerBase = new PrintManager(_sfDataGrid);
        _sfDataGrid.ShowPrintPreview();
    }

    private void OnExportToExcel()
    {
        ExcelExportManager.ExportToExcel(_sfDataGrid, _equipmentSheetName, _notificationManager);
    }

    private void OnExportToPdf()
    {
        PdfExportManager.ExportToPdf(_sfDataGrid, _equipmentSheetName, _notificationManager);
    }

    private void OnFilterItemsPopulated(GridFilterItemsPopulatedEventArgs e)
    {
        if (!_columnItemMap.TryGetValue(e.Column, out var columnItem))
            return;

        // Apply date format for filter popup
        if (columnItem.Settings.DataType == ColumnDataType.Date)
        {
            var dateSettings = columnItem.Settings.SpecificSettings as DateColumnSettings;
            var format = dateSettings?.DateFormat ?? "dd.MM.yyyy";
            foreach (var item in e.ItemsSource)
            {
                if (DateTime.TryParse(item.DisplayText, out DateTime parsedDate))
                {
                    item.DisplayText = parsedDate.ToString(format);
                }
            }
        }
    }

    private async Task OnImportFromExcelAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls",
            Title = "Виберіть файл Excel для імпорту",
            Multiselect = false
        };

        if (dlg.ShowDialog() != true) return;
        
        try
        {
            IsImporting = true;
            ImportProgress = 0;
            var progress = new Progress<int>(percent => ImportProgress = percent);
            
            var sheetNames = GetExcelSheetNames(dlg.FileName);
            if (sheetNames == null || sheetNames.Count == 0)
            {
                _notificationManager.Show("Файл не містить жодного листа", NotificationType.Warning);
                return;
            }
            
            string selectedSheet = String.Empty;
            int headerRow = 1;
            int headerCol = 1;

            if (sheetNames.Count > 1)
            {
                var selectionResult = await ShowSheetSelectorDialog(sheetNames);
                if (!selectionResult.HasValue) return;

                selectedSheet = selectionResult.Value.Sheet;
                headerRow = selectionResult.Value.Row;
                headerCol = selectionResult.Value.Col;
            }
            
            var importer = new EquipmentExcelImporter(
                _equipmentSheetService,
                _columnItemMap.Values,
                _tableId
            );
            
            var result = await importer.ImportAsync(
                filePath: dlg.FileName,
                sheetName: selectedSheet,
                headerRow: headerRow,
                headerCol: headerCol
            );
            
            await LoadRowsAsync();
            
            var message = result.Imported == 0 && result.Skipped > 0
                ? "Не знайдено даних для імпорту (перевірте вибір листа та рядка заголовків)"
                : $"Імпорт завершено: успішно {result.Imported}, пропущено {result.Skipped}";

            _notificationManager.Show(message,
                result.Imported > 0 ? NotificationType.Success : NotificationType.Warning);
        }
        catch (FileNotFoundException ex)
        {
            _notificationManager.Show($"Файл не знайдено: {ex.Message}", NotificationType.Error);
        }
        catch (IOException ex) when (IsFileLockedException(ex))
        {
            _notificationManager.Show(
                "Файл зайнятий іншим процесом. Будь ласка, закрийте його перед імпортом.",
                NotificationType.Warning);
        }
        catch (Exception ex)
        {
            _notificationManager.Show(
                $"Критична помилка імпорту: {ex.Message}",
                NotificationType.Error);
            _logger.LogError(ex, "Excel import failed");
        }
        finally
        {
            IsImporting = false;
            ImportProgress = 0;
        }
    }

    private async Task<(string Sheet, int Row, int Col)?> ShowSheetSelectorDialog(IList<string> sheetNames)
    {
        var tcs = new TaskCompletionSource<(string, int, int)?>();
        var parameters = new NavigationParameters
        {
            { "ScopedRegionManager", _scopedRegionManager },
            { "SheetNames", sheetNames },
            {
                "SelectionCallback", new Action<string, int, int, bool>((sheet, row, col, ok) =>
                {
                    _scopedRegionManager.Regions["SheetSelectorRegion"].RemoveAll();
                    tcs.SetResult(ok ? (sheet, row, col) : null);
                })
            }
        };
        _scopedRegionManager.RequestNavigate("SheetSelectorRegion", "SheetSelectorView", parameters);
        return await tcs.Task;;
    }

    private bool IsFileLockedException(Exception ex)
    {
        return ex is IOException && ex.Message.Contains("used by another process");
    }
    
    private IList<string> GetExcelSheetNames(string filePath)
    {
        try
        {
            using var engine = new ExcelEngine();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var workbook = engine.Excel.Workbooks.Open(stream);
            var sheets = workbook.Worksheets.Select(ws => ws.Name).ToList();
            workbook.Close();
            return sheets;
        }
        catch
        {
            return new List<string>();
        }
    }

    private void OnCurrentCellEndEdit(CurrentCellValidatingEventArgs e)
    {
    }

    private void OnUserControlLoaded(UserControl userControl)
    {
        _userControl = userControl;
    }
    
    private void OnAddNewRowInitiating(AddNewRowInitiatingEventArgs e)
    {
        if (e.NewObject is EquipmentRow newRow &&
            newRow.Data is IDictionary<string, object> rowDict)
        {
            foreach (var (gridCol, columnItem) in _columnItemMap)
            {
                if (columnItem.Settings.SpecificSettings is BooleanColumnSettings booleanSettings)
                {
                    var mapping = gridCol.MappingName;
                    rowDict[mapping] = booleanSettings.DefaultValue;
                }
            }
        }
    }

    private void OnRowValidating(RowValidatingEventArgs e)
    {
      // var validator = new RowValidator(_columnItemMap, Equipments);
       // var validationResult = validator.ValidateRow(e.RowData);
        
        //e.IsValid = validationResult.IsValid;

        //foreach (var error in validationResult.ErrorMessages)
        //{
        //    string columnMappingName = error.Key; 
         //   string errorMessage = error.Value;  
         //   e.ErrorMessages.Add(columnMappingName, errorMessage);
       // }
    }


    private async void OnRowValidated(RowValidatedEventArgs e)
    {
        /*if (e.RowData is ExpandoObject rowData)
        {
            var rowDict = (IDictionary<string, object?>)rowData;
            
            Guid id;
            if (_equipmentIds.TryGetValue(rowData, out Guid existingId))
            {
                id = existingId;
            }
            
            bool isNew = id == null;
            
            if (isNew)
            {
                Guid newId = await _equipmentDataGridService.InsertRowAsync(
                    _tableId,
                    e.RowIndex,
                    new Dictionary<string, object?>(rowDict)
                );
                
                _equipmentIds[rowData] = newId;
            }
            else
            {
                await _equipmentDataGridService.UpdateRowAsync(
                    id,
                    _tableId,
                    new Dictionary<string, object?>(rowDict)
                );
            }
        }*/
    }

    private void OnEditColumn(GridColumnContextMenuInfo contextInfo)
    {
        var parameters = new NavigationParameters
        {
            { NavigationConstants.ScopedRegionManager, _scopedRegionManager },
            { "BaseHeaderStyle", _baseGridHeaderStyle },
            { "EditingColumnItem", _columnItemMap[contextInfo.Column] },
            { "ColumnEditingCallback", new Action<ColumnEditingResult>(ColumnEditingCallback) }
        };
        _scopedRegionManager.RequestNavigate("ColumnCreatorRegion", "ColumnCreatorView", parameters);
    }

    private async void ColumnEditingCallback(ColumnEditingResult result)
    {
        /*try
        {
            _scopedRegionManager.Regions["ColumnCreatorRegion"].RemoveAll();
            if (!result.IsSuccessful) return;
            
          //  var editedColumnId = result.Column.Id;
          //  var shouldBePinned = result.Column.Settings.IsPinned;
            
            await _equipmentSheetService.UpdateColumnAsync(result.Column);
            
            await ReloadAllColumns();
            
            var editedColumn = Columns.FirstOrDefault(c => 
                _columnItemMap.TryGetValue(c, out var item) && item.Id == editedColumnId);
        
            if (editedColumn != null && shouldBePinned != _columnItemMap[editedColumn].Settings.IsPinned)
            {
                ToggleColumnPin(editedColumn, shouldBePinned);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ColumnEditingCallback");
            _notificationManager.Show($"Помилка при оновлені характеристики: {ex.Message}", NotificationType.Error);
        }*/
    }
    
    private async Task ReloadAllColumns()
    {
        var selectedItems = SelectedItems.ToList();
        
        Columns.Clear();
        _columnItemMap.Clear();
        _columnMappingNameIdMap.Clear();
        
        await LoadColumnsAsync();
        
        SelectedItems = new ObservableCollection<object>(selectedItems);
    }
    
    private void ToggleColumnPin(GridColumn column, bool pinStatus)
    {
        if (!_columnItemMap.TryGetValue(column, out var columnItem)) return;

        columnItem.Settings.IsPinned = pinStatus;
        Columns.Remove(column);

        if (pinStatus)
        {
            int firstPinnedPos = Columns
                .TakeWhile(c => _columnItemMap[c].Settings.IsPinned)
                .Count();
            Columns.Insert(firstPinnedPos, column);
        }
        else
        {
            Columns.Add(column);
        }

        FrozenColumnCount = Columns.Count(c => _columnItemMap[c].Settings.IsPinned);
    }

    private void OnEquipmentDataGridLoaded(SfDataGrid dataGrid)
    {
        _sfDataGrid = dataGrid;
        _sfDataGrid.RowDragDropController.Dropped += RowDragDropControllerOnDropped;
    }

    private void RowDragDropControllerOnDropped(object? sender, GridRowDroppedEventArgs e)
    {
    }

    private DateTime _lastClickTime = DateTime.MinValue;
    private bool _isDoubleClick = false;

    private async void OnCellTapped(CellTappedEventArgs e)
    {
        var now = DateTime.Now;
        if ((now - _lastClickTime).TotalMilliseconds < 300)
        {
            _isDoubleClick = true;
            return;
        }

        _lastClickTime = now;
        _isDoubleClick = false;

        await Task.Delay(300);
        if (_isDoubleClick)
            return;
        
        if (e.Column is not GridColumn column ||
            !_columnItemMap.TryGetValue(column, out var columnItem) ||
            columnItem.Settings.DataType != ColumnDataType.Hyperlink ||
            e.Record is not EquipmentRow row ||
            row.Data is not IDictionary<string, object> rowDict ||
            !rowDict.TryGetValue(column.MappingName, out var value))
            return;

        var url = value?.ToString();
        if (string.IsNullOrWhiteSpace(url))
            return;

        if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            url = "https://" + url;

        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            _notificationManager.Show($"Не вдалось відкрити посилання: {ex.Message}", NotificationType.Warning);
        }
    }

    private void OnActiveViewsChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        RaisePropertyChanged(nameof(IsOverlayVisible));
    }

    private void OnAddColumn()
    {
        var parameters = new NavigationParameters
        {
            { "ScopedRegionManager", _scopedRegionManager },
            { "BaseHeaderStyle", _baseGridHeaderStyle },
            { "ColumnCreatedCallback", new Action<ColumnCreationResult>(ColumnCreatedCallback) }
        };
        _scopedRegionManager.RequestNavigate("ColumnCreatorRegion", "ColumnCreatorView", parameters);
    }

    private async void ColumnCreatedCallback(ColumnCreationResult result)
    {
        try
        {
            _scopedRegionManager.Regions["ColumnCreatorRegion"].RemoveAll();

            if (!result.IsSuccessful)
                return;

           // int newColumnPosition = _columnMappingNameIdMap.Values.Any() ? _columnMappingNameIdMap.Values.Max() + 1 : 0;
           // result.ColumnSettings.ColumnPosition = newColumnPosition;
            //var columnItem = new ColumnItem
            {
           //     Id = await _equipmentSheetService.AddColumnAsync(result.ColumnSettings, _tableId),
           //     EquipmentSheetId = _tableId,
             //   Settings = result.ColumnSettings
            };
          //  var column = CreateColumn(result.ColumnSettings);
            
            // Pinned process
           // if (result.ColumnSettings.IsPinned)
            {
                int firstNonFrozenIndex = Columns
                    .TakeWhile(c => _columnItemMap.TryGetValue(c, out var item) && item.Settings.IsPinned)
                    .Count();
            
               // Columns.Insert(firstNonFrozenIndex, column);
                
                FrozenColumnCount = Columns.Count(c => 
                    _columnItemMap.TryGetValue(c, out var item) && item.Settings.IsPinned);
            }
            // Or default
           // else
            {
           //     Columns.Add(column);
            }
            
           // _columnMappingNameIdMap[columnItem.Settings.MappingName] = columnItem.Id;
           // _columnItemMap[column] = columnItem;
            
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ColumnCreatedCallback: {ex.Message}");
        }
    }

    private async void OnColumnDragging(QueryColumnDraggingEventArgs args)
    {
        if (args.OriginalSender is SfDataGrid sfDataGrid)
        {
            if (args.Reason == QueryColumnDraggingReason.Dropped)
            {
                var columnPositions = new Dictionary<Guid, int>();
                for (int i = 0; i < sfDataGrid.Columns.Count; i++)
                {
                    var column = sfDataGrid.Columns[i];
                    if (_columnMappingNameIdMap.TryGetValue(column.MappingName, out Guid columnId))
                    {
                        columnPositions[columnId] = i;
                        var item = _columnItemMap[column];
                        if (item != null)
                        {
                            item.Settings.ColumnPosition = i;
                        }
                    }
                }

                await _equipmentSheetService.UpdateColumnPositionAsync(columnPositions, _tableId);
            }
        }
    }

    private async void OnResizingColumn(ResizingColumnsEventArgs args)
    {
       /* if (args.OriginalSender is SfDataGrid sfDataGrid)
        {
            if (args.Reason == ColumnResizingReason.Resized)
            {
                var columnWidths = new Dictionary<int, double>();

                foreach (var column in sfDataGrid.Columns)
                {
                    if (_columnMappingNameIdMap.TryGetValue(column.MappingName, out Guid columnId))
                    {
                        columnWidths[columnId] = column.Width;
                    }
                }

                await _equipmentDataGridService.UpdateColumnWidthAsync(columnWidths, _tableId);
            }
        }*/
    }

    private async Task LoadColumnsAsync()
    { 
       /* var columnsDefinitions = await _equipmentDataGridService.GetColumnsAsync(_tableId);
        var orderedColumns = columnsDefinitions.OrderBy(c => c.Settings.ColumnPosition).ToList();

        var processed = await Task.Run(() =>
        {
            return columnsDefinitions.OrderBy(c => c.Settings.ColumnPosition)
                .Select(c => new
                {
                    ColumnDef = c,
                    ColumnItem = new ColumnItem
                    {
                        Id = c.Id,
                        TableId = _tableId,
                        Settings = c.Settings
                    }
                }).ToList();
        });

        foreach (var p in processed)
        {
            var column = CreateColumn(p.ColumnItem.Settings);
            Columns.Add(column);

            if (Columns.Count % 5 == 0)
            {
                await Task.Yield();
            }
        }
    
        FrozenColumnCount = orderedColumns.Count(c => c.Settings.IsPinned);*/
        
    }

    private async Task LoadRowsAsync()
    {
      /*  var rowsData = await _equipmentDataGridService.GetRowsAsync(_tableId);
        
        var columns = await _equipmentDataGridService.GetColumnsAsync(_tableId);
        var columnTypes = columns.ToDictionary(
            c => c.Settings.MappingName, 
            c => c.Settings.DataType
        );

        var processedData = await Task.Run(() =>
        {
            var tempProcessedList = new List<ExpandoObject>();
            var tempEquipmentIdsMap = new Dictionary<ExpandoObject, int>();

            foreach (var item in rowsData)
            {
                var data = new ExpandoObject();
                var dict = (IDictionary<string, object>)data;

                var itemDataDict = (IDictionary<string, object>)item.Data;
                foreach (var kvp in itemDataDict)
                {
                    dict[kvp.Key] = ConvertToProperType(kvp.Value, columnTypes.GetValueOrDefault(kvp.Key));
                }

                tempEquipmentIdsMap[data] = item.Id; 
                tempProcessedList.Add(data);
            }
            return (tempProcessedList, tempEquipmentIdsMap);
        });
        
        Equipments = new ObservableCollection<ExpandoObject>(processedData.tempProcessedList);
            
        _equipmentIds = new Dictionary<ExpandoObject, int>(processedData.tempEquipmentIdsMap);*/
        
    }
    
    private object? ConvertToProperType(object value, ColumnDataType dataType)
    {
        if (value == null || value is DBNull)
            return null;

        try
        {
            return dataType switch
            {
                ColumnDataType.Number => 
                    value is string s ? double.Parse(s, CultureInfo.InvariantCulture) : Convert.ToDouble(value),
                ColumnDataType.Boolean => 
                    value is string s ? bool.Parse(s) : Convert.ToBoolean(value),
                ColumnDataType.Date => 
                    value is string s ? DateTime.Parse(s) : Convert.ToDateTime(value),
                _ => value.ToString()
            };
        }
        catch
        {
            return value.ToString();
        }
    }
    

    private GridColumn CreateColumn(ColumnSettingsDisplayModel settings)
    {
        var headerTemplate = new CreateHeaderTemplateFactory();
        var headerStyle = new CreateHeaderStyleFactory();

        switch (settings.DataType)
        {
            case ColumnDataType.Number:
                var numberSpecificSettings = settings.SpecificSettings as NumberColumnSettings;
                return new GridNumericColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    NumberDecimalSeparator = ",",
                    NumberDecimalDigits = numberSpecificSettings.CharactersAfterComma,
                    MaxValue = (decimal)numberSpecificSettings.MaxValue,
                };
                break;
            case ColumnDataType.Currency:
                var currencySpecificSettings = settings.SpecificSettings as CurrencyColumnSettings;
                return new GridCurrencyColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    CurrencyDecimalDigits = 2,
                    CurrencyDecimalSeparator = ",",
                    CurrencySymbol = currencySpecificSettings.CurrencySymbol,
                };
                break;
            case ColumnDataType.Boolean:
                var checkboxSpecificSettings = settings.SpecificSettings as BooleanColumnSettings;
                return new GridCheckBoxColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                };
                break;
            case ColumnDataType.Date:
                var dateSpecificSettings = settings.SpecificSettings as DateColumnSettings;
                return new GridDateTimeColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    Pattern = DateTimePattern.CustomPattern,
                    CustomPattern = dateSpecificSettings.DateFormat,
                    AllowNullValue = true,
                };
                break;
            case ColumnDataType.List:
                var listSpecificSettings = settings.SpecificSettings as ListColumnSettings;
                return new GridComboBoxColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                    ItemsSource = listSpecificSettings.ListValues,
                };
                break;
            case ColumnDataType.Text:
                var textSpecificSettings = settings.SpecificSettings as TextColumnSettings;
                return new GridTextColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                };
            default:
                return new GridTextColumn
                {
                    HeaderText = settings.HeaderText,
                    TextAlignment = TextAlignment.Center,
                    TextWrapping = TextWrapping.Wrap,
                    MappingName = settings.MappingName,
                    AllowFiltering = settings.AllowFiltering,
                    AllowSorting = settings.AllowSorting,
                    AllowGrouping = settings.AllowGrouping,
                    AllowEditing = !settings.IsReadOnly,
                    Width = settings.ColumnWidth,
                };
            
        }
    }

    public void Destroy()
    {
        if (_scopedRegionManager != null)
        {
            if (_scopedRegionManager.Regions.ContainsRegionWithName("ColumnCreatorRegion"))
                _scopedRegionManager.Regions["ColumnCreatorRegion"].ActiveViews.CollectionChanged -= OnActiveViewsChanged;

            if (_scopedRegionManager.Regions.ContainsRegionWithName("SheetSelectorRegion"))
                _scopedRegionManager.Regions["SheetSelectorRegion"].ActiveViews.CollectionChanged -= OnActiveViewsChanged;
        }
        
        UnsubscribeFromEvents();
        

        _columnItemMap.Clear();
        _columnMappingNameIdMap.Clear();
        _columnItemMap.Clear();
        Columns = new ();
        Equipments = new();
        _sfDataGrid = null;
        _userControl = null;
        _baseGridHeaderStyle = null;
        _baseGridCellStyle = null;
        _scopedRegionManager = null;
    }

    public bool KeepAlive => true;
}