using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Services.EquipmentDataGrid;
using Models.EquipmentDataGrid;
using Notification.Wpf;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.UI.Xaml.Diagram;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Windows.Shared;
using Point = System.Drawing.Point;

namespace EquipmentTracker.ViewModels.DataGrid;

public class EquipmentDataGridViewModel: BindableBase, INavigationAware
{
    private readonly IAppLogger<EquipmentDataGridViewModel> _logger;
    private IRegionManager _scopedRegionManager;
    private readonly IEquipmentDataGridService _service;
    private readonly NotificationManager _globalNotificationManager;
    
    private string _equipmentTableName;
    
    private SfDataGrid _sparePartsDataGrid;
    private SfDataGrid _equipmentDataGrid;

    private Dictionary<string, bool> _visibleColumns = new();
    private ObservableCollection<EquipmentItem> _equipments = new();
    private EquipmentItem _selectedEquipment;

    private bool _isOverlayVisible;
    private bool _progressBarVisibility;
    
    // Search pannel fields
    private bool _searchPannelVisibility;
    private bool _isDragging;
    private string _searchText;
    private System.Windows.Point _startMousePos;
    private System.Windows.Point _startPanelPos;
    private double _searchPanelX = 0;
    private double _searchPanelY = 0;
    private SearchType _searchType = SearchType.Contains;
    private double _containerWidth;
    private double _containerHeight;
    private const double PanelWidth = 350;
    private const double PanelHeight = 140;

    public Dictionary<string, bool> VisibleColumns
    {
        get => _visibleColumns;
        set => SetProperty(ref _visibleColumns, value);
    }
    public ObservableCollection<EquipmentItem> Equipments
    {
        get => _equipments;
        set => SetProperty(ref _equipments, value);
    }
    public EquipmentItem SelectedEquipment
    {
        get => _selectedEquipment;
        set => SetProperty(ref _selectedEquipment, value);
    }
    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }

    public bool ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set => SetProperty(ref _progressBarVisibility, value);
    }

    // Search pannel properties
    public bool SearchPannelVisibility
    {
        get => _searchPannelVisibility;
        set => SetProperty(ref _searchPannelVisibility, value);
    }
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }
    public double ContainerWidth
    {
        get => _containerWidth;
        set => SetProperty(ref _containerWidth, value);
    }
    public double ContainerHeight
    {
        get => _containerHeight;
        set => SetProperty(ref _containerHeight, value);
    }
    public double SearchPanelX
    {
        get => _searchPanelX;
        set => SetProperty(ref _searchPanelX, value);
    }
    public double SearchPanelY
    {
        get => _searchPanelY;
        set => SetProperty(ref _searchPanelY, value);
    }
    public SearchType SearchType
    {
        get => _searchType;
        set
        {
            if (SetProperty(ref _searchType, value))
            {
                _equipmentDataGrid.SearchHelper.SearchType = value;
            }
        }
    }
    
    
    public DelegateCommand<SfDataGrid> EquipmentDataGridLoadedCommand { get; }
    public DelegateCommand<RowValidatingEventArgs> RowValidatingCommand { get; }
    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand { get; }
    public DelegateCommand<GridDetailsViewExpandingEventArgs> SparePartsLoadingCommand { get; }
    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand OpenSearchPannelCommand { get; }
    public DelegateCommand CloseSearchPannelCommand { get; }
    public DelegateCommand WriteOffCommand { get; }
    public DelegateCommand PrintCommand { get; }
    public DelegateCommand ExcelExportCommand { get; }
    public DelegateCommand PdfExportCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    public DelegateCommand<SfDataGrid> DetailsViewLoadingCommand { get; set; }
    
    // Search pannel command
    public Prism.Commands.DelegateCommand<MouseEventArgs> SearchPannelMouseMoveCommand { get; set; }
    public Prism.Commands.DelegateCommand<MouseButtonEventArgs> SearchPannelMouseDownCommand { get; set; }
    public Prism.Commands.DelegateCommand<MouseButtonEventArgs> SearchPannelMouseUpCommand { get; set; }
    
    public DelegateCommand<TextChangedEventArgs> SearchTextChangedCommand { get; }
    public DelegateCommand SearchNextCommand { get; }
    public DelegateCommand SearchPreviousCommand { get; }
    public DelegateCommand ClearSearchCommand { get; }
    
    public EquipmentDataGridViewModel(IAppLogger<EquipmentDataGridViewModel> logger,
        IEquipmentDataGridService service,
        NotificationManager globalNotificationManager)
    {
        _logger = logger;
        _service = service;
        _globalNotificationManager = globalNotificationManager;

        EquipmentDataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnEquipmentDataGridLoaded);
        RowValidatingCommand = new DelegateCommand<RowValidatingEventArgs>(async (args) => await OnRowValidating(args));
        RowValidatedCommand = new DelegateCommand<RowValidatedEventArgs>(async (args) => await OnRowValidated(args));
        SparePartsLoadingCommand = new DelegateCommand<GridDetailsViewExpandingEventArgs>(OnSparePartsLoading);
        RefreshCommand = new DelegateCommand(async (o) => await RefreshAsync());
        OpenSearchPannelCommand = new DelegateCommand(OnOpenSearchPannel);
        CloseSearchPannelCommand = new DelegateCommand(OnCloseSearchPannel);
        WriteOffCommand = new DelegateCommand(async (o) => await OnWriteOffEquipment());
        DeleteCommand = new DelegateCommand(async (o) => await OnDeleteEquipment());
        DetailsViewLoadingCommand = new DelegateCommand<SfDataGrid>(SparePartsDataGridLoading);
        PrintCommand = new DelegateCommand(OnPrint);
        
        // Search pannel command initialization
        SearchPannelMouseDownCommand = new Prism.Commands.DelegateCommand<MouseButtonEventArgs>(OnSearchPannelMouseDown);
        SearchPannelMouseMoveCommand = new Prism.Commands.DelegateCommand<MouseEventArgs>(OnSearchPannelMouseMove);
        SearchPannelMouseUpCommand = new Prism.Commands.DelegateCommand<MouseButtonEventArgs>(OnSearchPannelMouseUp);
        SearchTextChangedCommand = new DelegateCommand<TextChangedEventArgs>(OnSearchTextChanged);
        SearchNextCommand = new DelegateCommand(OnSearchNext);
        SearchPreviousCommand = new DelegateCommand(OnSearchPrevious);
        ClearSearchCommand = new DelegateCommand(OnClearSearch);
    }
    
    private void OnClearSearch(object obj)
    {
        _equipmentDataGrid.SearchHelper.ClearSearch();
        _equipmentDataGrid.SelectionController.ClearSelections(false);
        SearchText = string.Empty;
        SearchType = SearchType.Contains;
    }
    
    private void OnSearchPrevious(object obj)
    {
        _equipmentDataGrid.SearchHelper.FindPrevious(SearchText);
        _equipmentDataGrid.SelectionController.MoveCurrentCell(_equipmentDataGrid.SearchHelper.CurrentRowColumnIndex);
    }

    private void OnSearchNext(object obj)
    {
        _equipmentDataGrid.SearchHelper.FindNext(SearchText);
        _equipmentDataGrid.SelectionController.MoveCurrentCell(_equipmentDataGrid.SearchHelper.CurrentRowColumnIndex);
    }

    private void OnPrint(object obj)
    {
        _equipmentDataGrid.ShowPrintPreview();
    }

    private void OnEquipmentDataGridLoaded(SfDataGrid equipmentDataGrid)
    {
        _equipmentDataGrid = equipmentDataGrid;
        _equipmentDataGrid.SearchHelper.AllowFiltering = true;
        Console.WriteLine(_equipmentDataGrid.SearchHelper.SearchType);
    }

    private void OnSearchTextChanged(TextChangedEventArgs obj)
    {
        _equipmentDataGrid.SearchHelper.Search(SearchText);
        _equipmentDataGrid.SelectionController.ClearSelections(false);
    }

    private void OnSearchPannelMouseDown(MouseButtonEventArgs args)
    {
        if (args.LeftButton == MouseButtonState.Pressed)
        {
            _isDragging = true;
            _startMousePos = args.GetPosition(null);
            _startPanelPos = new System.Windows.Point(SearchPanelX, SearchPanelY);
        }
    }

    private void OnSearchPannelMouseMove(MouseEventArgs args)
    {
        if (!_isDragging) return;

        var currentPos = args.GetPosition(null);
        var offsetX = currentPos.X - _startMousePos.X;
        var offsetY = currentPos.Y - _startMousePos.Y;

        var newX = _startPanelPos.X + offsetX;
        var newY = _startPanelPos.Y + offsetY;
        
        if (ContainerWidth > 0 && ContainerHeight > 0)
        {
            newX = Math.Max(0, Math.Min(ContainerWidth - PanelWidth, newX));
            newY = Math.Max(0, Math.Min(ContainerHeight - PanelHeight, newY));
        }

        SearchPanelX = newX;
        SearchPanelY = newY;
    }

    private void OnSearchPannelMouseUp(MouseButtonEventArgs args)
    {
        _isDragging = false;
    }
    private void OnOpenSearchPannel(object obj)
    {
       SearchPannelVisibility = true;
    }

    private void OnCloseSearchPannel(object obj)
    {
        SearchPannelVisibility = false;
    }
    

    private async void OnSparePartsLoading(GridDetailsViewExpandingEventArgs args)
    {
        if (args.Record is EquipmentItem equipmentItem)
        {
            string sparePartsTableName = $"{_equipmentTableName} ЗЧ";
            var spareParts = await _service.GetSparePartItemAsync(equipmentItem.Id, sparePartsTableName);
            
            equipmentItem.SpareParts.Clear(); 
            foreach (var part in spareParts)
            {
                equipmentItem.SpareParts.Add(part); 
            }
        }
    }

    private void SparePartsDataGridLoading(SfDataGrid sparePartsDataGrid)
    {
        _sparePartsDataGrid = sparePartsDataGrid;
        _sparePartsDataGrid.RowValidating += SpareParts_RowValidating;
        _sparePartsDataGrid.RowValidated += SpareParts_RowValidated;
    }
    
    private void SpareParts_RowValidating(object? sender, RowValidatingEventArgs args)
    {
        var rowData = args.RowData as SparePartItem;
        if(rowData == null) return;
        args.ErrorMessages.Clear();

        string? error;
        
        void Add(string key, string? message)
        {
            args.IsValid = false;
            if (message != null)
                args.ErrorMessages.Add(key, message);
        }
        
        if(!IsValidText(rowData.SparePartName, 3, 50, out error, "Назва", true)) Add ("SparePartName", error);
        if(!IsValidText(rowData.SparePartCategory, 3, 50, out error, "Категорія", false)) Add ("SparePartCategory", error);
        if(!IsValidText(rowData.SparePartSerialNumber, 3, 50, out error, "Серійний номер", false)) Add ("SparePartSerialNumber", error);
        if(!IsValidText(rowData.SparePartUnit, 1, 50, out error, "Одиниця")) Add ("SparePartUnit", error);
        if(!IsValidText(rowData.SparePartNotes, 0, 2000, out error, "Нотатки", false)) Add ("SparePartNotes", error);
        if(!IsValidNumber(rowData.SparePartQuantity, 0, 10000, out error, "Кількість", true)) Add ("SparePartQuantity", error); 
    }

    private async void SpareParts_RowValidated(object? sender, RowValidatedEventArgs args)
    {
        var rowData = args.RowData as SparePartItem;
        if(rowData == null) return;
        string sparePartsTableName = $"{_equipmentTableName} ЗЧ";

        if (rowData.Id == 0)
        {
            rowData.EquipmentId = SelectedEquipment.Id;
            rowData.Id = await _service.InsertSparePartAsync(rowData, sparePartsTableName);
        }
        else
        {
            await _service.UpdateSparePartAsync(rowData, sparePartsTableName);
        }
    }

    #region Row Validating
    private async Task OnRowValidating(RowValidatingEventArgs args)
    {
        var rowData = args.RowData as EquipmentItem;
        if(rowData == null) return;
        args.IsValid = true;
        
        string? error;
        
        void Add(string key, string? message)
        {
            args.IsValid = false;
            if (message != null)
                args.ErrorMessages.Add(key, message);
        }
        
        if (IsColumnVisible("Brand") && !IsValidText(rowData.Brand, 2, 50, out error, "Бренд")) Add("Brand", error);
        
        if (IsColumnVisible("InventoryNumber") && !IsValidText(rowData.InventoryNumber, 3, 12, out error, "Інвентарний номер")) Add("InventoryNumber", error);
        if (IsColumnVisible("Model") && !IsValidText(rowData.Model, 2, 50, out error, "Модель")) Add("Model", error);
        if (IsColumnVisible("Category") && !IsValidText(rowData.Category, 2, 50, out error, "Категорія")) Add("Category", error);
        if (IsColumnVisible("SerialNumber") && !IsValidText(rowData.SerialNumber, 0, 50, out error, "Серійний номер", false)) Add("SerialNumber", error);
        if (IsColumnVisible("Class") && !IsValidText(rowData.Class, 0, 50, out error, "Клас", false)) Add("Class", error);
        if (IsColumnVisible("Year") && rowData.Year.HasValue && !IsValidText(rowData.Year.Value.ToString(), 4, 4, out error, "Рік", false)) Add("Year", error);

        if (IsColumnVisible("Height") && !IsValidNumber(rowData.Height, 0, 10000, out error, "Висота")) Add("Height", error);
        if (IsColumnVisible("Width") && !IsValidNumber(rowData.Width, 0, 10000, out error, "Ширина")) Add("Width", error);
        if (IsColumnVisible("Length") && !IsValidNumber(rowData.Length, 0, 10000, out error, "Довжина")) Add("Length", error);
        if (IsColumnVisible("Weight") && !IsValidNumber(rowData.Weight, 0, 100000, out error, "Вага")) Add("Weight", error);

        if (IsColumnVisible("Floor") && !IsValidText(rowData.Floor, 0, 20, out error, "Поверх", false)) Add("Floor", error);
        if (IsColumnVisible("Department") && !IsValidText(rowData.Department, 0, 50, out error, "Відділ", false)) Add("Department", error);
        if (IsColumnVisible("Room") && !IsValidText(rowData.Room, 0, 20, out error, "Кімната", false)) Add("Room", error);

        if (IsColumnVisible("Consumption") && !IsValidNumber(rowData.Consumption, 0, 100000, out error, "Споживання")) Add("Consumption", error);
        if (IsColumnVisible("Voltage") && !IsValidNumber(rowData.Voltage, 0, 100000, out error, "Напруга")) Add("Voltage", error);
        if (IsColumnVisible("Water") && !IsValidNumber(rowData.Water, 0, 100000, out error, "Вода")) Add("Water", error);
        if (IsColumnVisible("Air") && !IsValidNumber(rowData.Air, 0, 100000, out error, "Повітря")) Add("Air", error);

        if (IsColumnVisible("BalanceCost") && !IsValidNumber(rowData.BalanceCost, 0, 1000000000, out error, "Балансова вартість")) Add("BalanceCost", error);

        if (IsColumnVisible("Notes") && !IsValidText(rowData.Notes, 0, 2000, out error, "Нотатки", false)) Add("Notes", error);
        if (IsColumnVisible("ResponsiblePerson") && !IsValidText(rowData.ResponsiblePerson, 0, 100, out error, "Відповідальний", false)) Add("ResponsiblePerson", error);
    }
    
    private bool IsColumnVisible(string columnName)
    {
        // If the column is NOT found in the dictionary or its value is false, we consider it NOT visible
        if (!VisibleColumns.TryGetValue(columnName, out bool isVisible))
            return false;
        return isVisible;
    }
    
    bool IsValidText(string value, int min, int max, out string? error, string label, bool required = true)
    {
        error = null;
        if (string.IsNullOrWhiteSpace(value))
        {
            if (required)
            {
                error = $"{label} є обов'язковим полем";
                return false;
            }
            return true;
        }

        if (value.Length < min)
        {
            error = $"{label} має містити мінімум {min} символів";
            return false;
        }

        if (value.Length > max)
        {
            error = $"{label} може містити максимум {max} символів";
            return false;
        }

        if (!Regex.IsMatch(value, @"^[\p{L}\d\s\-]+$"))
        {
            error = $"{label} містить недопустимі символи";
            return false;
        }

        return true;
    }
    
    
    bool IsValidNumber(decimal? value, decimal min, decimal max, out string? error, string label, bool required = false)
    {
        error = null;
        if (!value.HasValue)
        {
            if (required)
            {
                error = $"{label} є обов'язковим полем";
                return false;
            }
            return true;
        }

        if (value < min || value > max)
        {
            error = $"{label} має бути в діапазоні від {min} до {max}";
            return false;
        }

        return true;
    }
    #endregion

    private async Task OnRowValidated(RowValidatedEventArgs args)
    {
        var rowData = args.RowData as EquipmentItem;
        if (rowData == null) return;

        if (rowData.Id == 0)
        {
            rowData.Id = await _service.InsertEquipmentAsync(rowData, _equipmentTableName);
        }
        else
        {
            await _service.UpdateEquipmentAsync(rowData, _equipmentTableName);
        }
    }
    
    private async Task LoadColumnsAsync()
    {
        try
        {
            VisibleColumns = await _service.GetVisibleColumnsAsync(_equipmentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load columns");
            throw;
        }
    }

    private async Task OnWriteOffEquipment()
    {
        if (SelectedEquipment is EquipmentItem equipmentItem)
        {
            try
            {
                await _service.WriteOffEquipmentAsync(equipmentItem.Id, _equipmentTableName);
                Equipments.Remove(equipmentItem);
                _globalNotificationManager.Show("Успішно додано в списані", NotificationType.Success);
            }
            catch (Exception e)
            {
                _globalNotificationManager.Show($"Помилка додавання в списані: {e.Message}", NotificationType.Error);
                _logger.LogError(e, "Failed to writeoff equipment");
                throw;
            }
        }
    }

    private async Task OnDeleteEquipment()
    {
        if (SelectedEquipment is EquipmentItem equipmentItem)
        {
            IsOverlayVisible = true;
            var parameters = new NavigationParameters();
            parameters.Add("Callback", new Action<IDeletionAgreementResult>(async result=>
                {
                    if (result.Result.HasValue && result.Result.Value)
                    {
                        try
                        {
                            await _service.MakeDataCopyAsync(equipmentItem.Id, _equipmentTableName);
                            Equipments.Remove(equipmentItem);
                            _globalNotificationManager.Show("Успішно видалено!", NotificationType.Success);
                        }
                        catch (Exception e)
                        {
                            _globalNotificationManager.Show($"Помилка видалення: {e.Message}", NotificationType.Error);
                            _logger.LogError(e, "Failed to delete equipment");
                            throw;
                        }
                        _scopedRegionManager.Regions["DeletionAgreementRegion"].RemoveAll();
                    }
                    else
                    {
                        _scopedRegionManager.Regions["DeletionAgreementRegion"].RemoveAll();
                    }
                    IsOverlayVisible = false;
                }
                ));
            _scopedRegionManager.RequestNavigate("DeletionAgreementRegion", "DeletionAgreementView", parameters);
        }
    }

    private async Task LoadEquipmentsAsync()
    {
        ProgressBarVisibility = true;
        await Task.Delay(500);
        try
        {
            Equipments = await _service.GetEquipmentItemsAsync(_equipmentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to equipments columns");
            throw;
        }
        finally
        {
            ProgressBarVisibility = false;
        }
    }

    private async Task RefreshAsync()
    {
        await LoadEquipmentsAsync();
    }
    

    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["TableName"] is string tableName)
        {
            _equipmentTableName = tableName;
        }
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager regionManager)
        {
            _scopedRegionManager = regionManager;
        }

        await LoadColumnsAsync();
        await LoadEquipmentsAsync();
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}