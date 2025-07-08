using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using System.IO;
using System.Linq;
using Syncfusion.XlsIO;
using Core.Services.EquipmentDataGrid;
using Models.Equipment;
using Models.Equipment.ColumnCreator;
using Models.Equipment.ColumnSpecificSettings;
using Notification.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Syncfusion.SfSkinManager;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Windows.Shared;
using Syncfusion.XPS;
using UI.ViewModels.Equipment.DataGrid.TemplateCreators;
using ColorConverter = System.Windows.Media.ColorConverter;
using DelegateCommand = Prism.Commands.DelegateCommand;
using FontFamily = System.Drawing.FontFamily;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace UI.ViewModels.Equipment.DataGrid;

public class EquipmentDataGridViewModel : BindableBase, INavigationAware
{
    private readonly IEquipmentDataGridService _equipmentDataGridService;
    private readonly NotificationManager _notificationManager;
    private IRegionManager _scopedRegionManager;
    private int _tableId;

    private Dictionary<string, int> _columnMappingNameIdMap = new();
    private Dictionary<GridColumnBase, ColumnItem> _columnItemMap = new();
    private Dictionary<DataColumnBase, DataColumn> _columnDataColumnMap = new();

    private ObservableCollection<EquipmentRow> _equipments = new();

    public ObservableCollection<EquipmentRow> Equipments
    {
        get => _equipments;
        set => SetProperty(ref _equipments, value);
    }

    private bool _progressBarVisibility;

    public bool ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set => SetProperty(ref _progressBarVisibility, value);
    }

    private SfDataGrid _sfDataGrid;
    private UserControl _userControl;

    private Style _baseGridHeaderStyle;
    private Style _baseGridCellStyle;
    public bool IsOverlayVisible =>
        _scopedRegionManager.Regions["ColumnCreatorRegion"].ActiveViews.Any() ||
        _scopedRegionManager.Regions["SheetSelectorRegion"].ActiveViews.Any();

    public Columns Columns { get; } = new();

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _scopedRegionManager = scopedRegionManager;

            if (_scopedRegionManager.Regions.ContainsRegionWithName("ContentRegion"))
            {
                var region = _scopedRegionManager.Regions["ContentRegion"].ActiveViews.FirstOrDefault() as UserControl;
                if (region != null)
                {
                    var baseHeaderStyle = region.TryFindResource("SyncfusionGridHeaderCellControlStyle") as Style;
                    var baseCellStyle = region.TryFindResource("SyncfusionGridCellStyle") as Style;
                    _baseGridHeaderStyle = baseHeaderStyle;
                    _baseGridCellStyle = baseCellStyle;
                }
            }

            Console.WriteLine("OnNavigatedTo");
            if (_scopedRegionManager.Regions.ContainsRegionWithName("ColumnCreatorRegion"))
            {
                _scopedRegionManager.Regions["ColumnCreatorRegion"].ActiveViews.CollectionChanged += OnActiveViewsChanged;

                RaisePropertyChanged(nameof(IsOverlayVisible));
            }

            if (_scopedRegionManager.Regions.ContainsRegionWithName("SheetSelectorRegion"))
            {
                _scopedRegionManager.Regions["SheetSelectorRegion"].ActiveViews.CollectionChanged += OnActiveViewsChanged;
            }
        }

        if (navigationContext.Parameters["TableId"] is int tableId)
        {
            _tableId = tableId;
        }

        try
        {
            ProgressBarVisibility = true;
            await LoadColumnsAsync();
            await LoadRowsAsync();
        }
        finally
        {
            await Task.Delay(500);
            ProgressBarVisibility = false;
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
    public DelegateCommand ImportFromExcelCommand { get; }

    public EquipmentDataGridViewModel(IEquipmentDataGridService equipmentDataGridService,
        NotificationManager notificationManager)
    {
        _equipmentDataGridService = equipmentDataGridService;
        _notificationManager = notificationManager;

        ColumnDraggingCommand = new Prism.Commands.DelegateCommand<QueryColumnDraggingEventArgs>(OnColumnDragging);
        ResizingColumnCommand = new Prism.Commands.DelegateCommand<ResizingColumnsEventArgs>(OnResizingColumn);
        AddColumnCommand = new DelegateCommand(OnAddColumn);
        EquipmentDataGridLoadedCommand = new Prism.Commands.DelegateCommand<SfDataGrid>(OnEquipmentDataGridLoaded);
        EditColumnCommand = new Prism.Commands.DelegateCommand<GridColumnContextMenuInfo>(OnEditColumn);
        RowValidatingCommand = new Prism.Commands.DelegateCommand<RowValidatingEventArgs>(OnRowValidating);
        RowValidatedCommand = new Prism.Commands.DelegateCommand<RowValidatedEventArgs>(OnRowValidated);
        CellTappedCommand = new Prism.Commands.DelegateCommand<CellTappedEventArgs>(OnCellTapped);
        AddNewRowInitiatingCommand = new Prism.Commands.DelegateCommand<AddNewRowInitiatingEventArgs>(OnAddNewRowInitiating);
        UserControlLoadedCommand = new Prism.Commands.DelegateCommand<UserControl>(OnUserControlLoaded);
        CurrentCellEndEditCommand = new Prism.Commands.DelegateCommand<CurrentCellValidatingEventArgs>(OnCurrentCellEndEdit);
        ImportFromExcelCommand = new DelegateCommand(async () => await OnImportFromExcelAsync());
    }

    private async Task OnImportFromExcelAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Excel files (*.xlsx;*.xls)|*.xlsx;*.xls",
            Title = "Виберіть файл Excel для імпорту"
        };
        if (dlg.ShowDialog() != true)
            return;

        try
        {
            ProgressBarVisibility = true;
            // открыть диалог выбора листа
            var wbSheets = GetExcelSheetNames(dlg.FileName);
            string? selectedSheet = null;
int headerRow = 1;
int headerCol = 1;
            if (wbSheets.Count > 1)
            {
                var tcs = new TaskCompletionSource<(string sheet,int row,int col)?>();
                var parameters = new NavigationParameters
                {
                    { "ScopedRegionManager", _scopedRegionManager },
                    { "SheetNames", wbSheets },
                    { "SelectionCallback", new Action<string?, int, int, bool>((sheet, row, col, ok) =>
                        {
                            _scopedRegionManager.Regions["SheetSelectorRegion"].RemoveAll();
                            tcs.SetResult(ok ? (sheet, row, col) : null);
                        }) }
                };
                _scopedRegionManager.RequestNavigate("SheetSelectorRegion", "SheetSelectorView", parameters);
                var res = await tcs.Task;
                if (res == null)
                    return;
                selectedSheet = res?.sheet;
                headerRow = res?.row ?? 1;
                headerCol = res?.col ?? 1;
            }

            var importer = new Import.EquipmentExcelImporter(_equipmentDataGridService, _columnItemMap.Values, _tableId);
            var result = await importer.ImportAsync(dlg.FileName, selectedSheet, headerRow, headerCol);

            await LoadRowsAsync();
            _notificationManager.Show($"Імпорт завершено: успішно {result.Imported}, пропущено {result.Skipped}", NotificationType.Success);
        }
        catch (Exception ex)
        {
            _notificationManager.Show($"Помилка імпорту: {ex.Message}", NotificationType.Error);
        }
        finally
        {
            ProgressBarVisibility = false;
        }
    }

    private void OnCurrentCellEndEdit(CurrentCellValidatingEventArgs e)
    {
        Console.WriteLine("OnCurrentCellEndEdit");
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
        var validator = new RowValidator(_columnItemMap);
        var validationResult = validator.ValidateRow(e.RowData);
        
        e.IsValid = validationResult.IsValid;

        foreach (var error in validationResult.ErrorMessages)
        {
            string columnMappingName = error.Key; 
            string errorMessage = error.Value;  
            e.ErrorMessages.Add(columnMappingName, errorMessage);
        }
    }


    private async void OnRowValidated(RowValidatedEventArgs e)
    {
        if (e.RowData is EquipmentRow row &&
            row.Data is IDictionary<string, object> rowDict)
        {
            bool isNew = row.Id == 0;
            if (isNew)
            {
                var equipment = new EquipmentItem
                {
                    TableId = _tableId,
                    RowIndex = e.RowIndex,
                    Data = rowDict,
                };
                int newId = await _equipmentDataGridService.AddNewRowAsync(equipment);
                row.Id = newId;
            }
            else
            {
                await _equipmentDataGridService.UpdateRowsAsync(rowDict, row.Id);
            }
        }
    }

    private void OnEditColumn(GridColumnContextMenuInfo contextInfo)
    {
        var parameters = new NavigationParameters
        {
            { "ScopedRegionManager", _scopedRegionManager },
            { "BaseHeaderStyle", _baseGridHeaderStyle },
            { "EditingColumnItem", _columnItemMap[contextInfo.Column] },
            { "ColumnEditingCallback", new Action<ColumnEditingResult>(ColumnEditingCallback) }
        };
        _scopedRegionManager.RequestNavigate("ColumnCreatorRegion", "ColumnCreatorView", parameters);
    }

    private async void ColumnEditingCallback(ColumnEditingResult result)
    {
        try
        {
            _scopedRegionManager.Regions["ColumnCreatorRegion"].RemoveAll();
            if (!result.IsSuccessful)
                return;
            await _equipmentDataGridService.UpdateColumnAsync(result.Column);

            await LoadColumnsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in ColumnEditingCallback: {ex.Message}");
            throw;
        }
    }

    private void OnEquipmentDataGridLoaded(SfDataGrid dataGrid)
    {
        _sfDataGrid = dataGrid;
        _sfDataGrid.RowDragDropController.Dropped += RowDragDropControllerOnDropped;
    }

    private void RowDragDropControllerOnDropped(object? sender, GridRowDroppedEventArgs e)
    {
        Console.WriteLine(e.TargetRecord);
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

            int newColumnPosition = _columnMappingNameIdMap.Values.Any() ? _columnMappingNameIdMap.Values.Max() + 1 : 0;
            result.ColumnSettings.ColumnPosition = newColumnPosition;
            var columnItem = new ColumnItem
            {
                Id = await _equipmentDataGridService.AddColumnAsync(result.ColumnSettings, _tableId),
                TableId = _tableId,
                Settings = result.ColumnSettings
            };
            var column = CreateColumn(result.ColumnSettings);
            Columns.Add(column);
            _columnMappingNameIdMap[columnItem.Settings.MappingName] = columnItem.Id;
            _columnItemMap[column] = columnItem;
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
                var columnPositions = new Dictionary<int, int>();
                for (int i = 0; i < sfDataGrid.Columns.Count; i++)
                {
                    var column = sfDataGrid.Columns[i];
                    if (_columnMappingNameIdMap.TryGetValue(column.MappingName, out int columnId))
                    {
                        columnPositions[columnId] = i;
                        var item = _columnItemMap[column];
                        if (item != null)
                        {
                            item.Settings.ColumnPosition = i;
                        }
                    }
                }

                await _equipmentDataGridService.UpdateColumnPositionAsync(columnPositions, _tableId);
            }
        }
    }

    private async void OnResizingColumn(ResizingColumnsEventArgs args)
    {
        if (args.OriginalSender is SfDataGrid sfDataGrid)
        {
            if (args.Reason == ColumnResizingReason.Resized)
            {
                var columnWidths = new Dictionary<int, double>();

                foreach (var column in sfDataGrid.Columns)
                {
                    if (_columnMappingNameIdMap.TryGetValue(column.MappingName, out int columnId))
                    {
                        columnWidths[columnId] = column.Width;
                    }
                }

                await _equipmentDataGridService.UpdateColumnWidthAsync(columnWidths, _tableId);
            }
        }
    }

    private async Task LoadColumnsAsync()
    {
        var columnsDefinitions = await _equipmentDataGridService.GetColumnsAsync(_tableId);

        Columns.Clear();
        _columnItemMap.Clear();
        _columnMappingNameIdMap.Clear();

        foreach (var columnDef in columnsDefinitions.OrderBy(c => c.Settings.ColumnPosition))
        {
            var columnItem = new ColumnItem
            {
                Id = columnDef.Id,
                TableId = _tableId,
                Settings = columnDef.Settings
            };

            var column = CreateColumn(columnDef.Settings);

            Columns.Add(column);
            _columnMappingNameIdMap[columnDef.Settings.MappingName] = columnItem.Id;
            _columnItemMap[column] = columnItem;
        }
    }

    private async Task LoadRowsAsync()
    {
        var rows = await _equipmentDataGridService.GetRowsAsync(_tableId);

        Equipments = new ObservableCollection<EquipmentRow>(
            rows.Select(item =>
            {
                var data = new ExpandoObject();
                var dict = (IDictionary<string, object>)data;

                foreach (var kvp in item.Data)
                    dict[kvp.Key] = kvp.Value;

                return new EquipmentRow
                {
                    Id = item.Id,
                    Data = data
                };
            }).ToList()
        );
    }
    

    private GridColumn CreateColumn(ColumnSettings settings)
    {
        var headerTemplate = new CreateHeaderTemplateFactory();
        var headerStyle = new CreateHeaderStyleFactory();
        var cellTemplate = new CreateCellTemplateFactory();
        var editTemplate = new CreateEditTemplateFactory();
        
        var column = new GridTemplateColumn
        {
            HeaderText = settings.HeaderText,
            MappingName = settings.MappingName,
            AllowFiltering = settings.AllowFiltering,
            AllowSorting = settings.AllowSorting,
            AllowGrouping = settings.AllowGrouping,
            AllowEditing = !settings.IsReadOnly,
            Width = settings.ColumnWidth,
            HeaderTemplate = headerTemplate.CreateHeaderTemplate(settings),
            HeaderStyle = headerStyle.CreateHeaderStyle(settings, _baseGridHeaderStyle),
            CellTemplate = cellTemplate.CreateCellTemplate(settings),
            EditTemplate = editTemplate.CreateEditTemplate(settings)
        };
        return column;
    }
    private static List<string> GetExcelSheetNames(string filePath)
    {
        using var engine = new ExcelEngine();
        var app = engine.Excel;
        using var fs = File.OpenRead(filePath);
        var wb = app.Workbooks.Open(fs);
        var list = wb.Worksheets.Select(ws => ws.Name).ToList();
        wb.Close();
        return list;
    }
}