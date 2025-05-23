﻿using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Services.EquipmentDataGrid;
using Models.EquipmentDataGrid;
using Notification.Wpf;
using Prism.Mvvm;
using Prism.Regions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Shared;

namespace EquipmentTracker.ViewModels.DataGrid;

public class EquipmentDataGridViewModel: BindableBase, INavigationAware
{
    private readonly IAppLogger<EquipmentDataGridViewModel> _logger;
    private IRegionManager _scopedRegionManager;
    private readonly IEquipmentDataGridService _service;
    private readonly NotificationManager _globalNotificationManager;
    
    private string _equipmentTableName;

    private Dictionary<string, bool> _visibleColumns = new();
    private ObservableCollection<EquipmentItem> _equipments = new();
    private EquipmentItem _selectedEquipment;

    private bool _isOverlayVisible;
    private bool _progressBarVisibility;

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
    
    public DelegateCommand<RowValidatingEventArgs> RowValidatingCommand { get; }
    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand { get; }
    public DelegateCommand<RowValidatingEventArgs> SparePartsRowValidatingCommand { get; }
    public DelegateCommand<GridDetailsViewExpandingEventArgs> SparePartsLoadingCommand { get; }
    public DelegateCommand RefreshCommand { get; }
    public DelegateCommand WriteOffCommand { get; }
    public DelegateCommand PrintCommand { get; }
    public DelegateCommand ExcelExportCommand { get; }
    public DelegateCommand PdfExportCommand { get; }
    public DelegateCommand DeleteCommand { get; }
    
    public EquipmentDataGridViewModel(IAppLogger<EquipmentDataGridViewModel> logger,
        IEquipmentDataGridService service,
        NotificationManager globalNotificationManager)
    {
        _logger = logger;
        _service = service;
        _globalNotificationManager = globalNotificationManager;

        RowValidatingCommand = new DelegateCommand<RowValidatingEventArgs>(async (args) => await OnRowValidating(args));
        RowValidatedCommand = new DelegateCommand<RowValidatedEventArgs>(async (args) => await OnRowValidated(args));
        SparePartsLoadingCommand = new DelegateCommand<GridDetailsViewExpandingEventArgs>(OnSparePartsLoading);
        RefreshCommand = new DelegateCommand(async (o) => await RefreshAsync());
        WriteOffCommand = new DelegateCommand(async (o) => await OnWriteOffEquipment());
        DeleteCommand = new DelegateCommand(async (o) => await OnDeleteEquipment());
        SparePartsRowValidatingCommand = new DelegateCommand<RowValidatingEventArgs>(test);
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

    private void test(RowValidatingEventArgs args)
    {
        Console.WriteLine("test");
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
        
        if (IsColumnVisible("InventoryNumber") && !IsValidText(rowData.InventoryNumber, 3, 12, out error, "Інвентарний номер")) Add("InventoryNumber", error);
        if (IsColumnVisible("Brand") && !IsValidText(rowData.Brand, 2, 50, out error, "Бренд")) Add("Brand", error);
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