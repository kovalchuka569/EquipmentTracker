using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Common.Logging;
using Core.Interfaces;
using Notification.Wpf;
using Presentation.ViewModels.Common.PivotGrid;
using Prism.Commands;
using Prism.Navigation.Regions;
using JetBrains.Annotations;
using Models.Common.Table;
using Models.Equipment;
using Presentation.ViewModels.Common.Table;
using Syncfusion.PivotAnalysis.Base;
using Syncfusion.Windows.Controls.PivotSchemaDesigner;

namespace Presentation.ViewModels;

public class PivotSheetViewModel : BaseViewModel<PivotSheetViewModel>, INavigationAware
{
    
    #region Dependencies

    private IEquipmentSheetService _equipmentSheetService;
    
    #endregion
    
    #region Private fields
    
    private Guid _pivotSheetId;
    
    private bool _isInitialized;
    
    private DataView _pivotData = new();
    
    private ObservableCollection<PivotItem> _pivotColumns = new();
    
    private ObservableCollection<PivotItem> _pivotRows = new();
    
    private ObservableCollection<PivotComputationInfo> _pivotCalculations = new();
    
    #endregion
    
    #region Public fields

    public DataView PivotData
    {
        get => _pivotData;
        set => SetProperty(ref _pivotData, value);
    }

    public ObservableCollection<PivotItem> PivotColumns
    {
        get => _pivotColumns;
        set => SetProperty(ref _pivotColumns, value);
    }

    public ObservableCollection<PivotItem> PivotRows
    {
        get => _pivotRows;
        set => SetProperty(ref _pivotRows, value);
    }

    public ObservableCollection<PivotComputationInfo> PivotCalculations
    {
        get => _pivotCalculations;
        set => SetProperty(ref _pivotCalculations, value);
    }
    
    
    #endregion
    
    #region Constructor
    
    public PivotSheetViewModel(NotificationManager notificationManager, 
        IAppLogger<PivotSheetViewModel> logger,
        IEquipmentSheetService equipmentSheetService) : base(notificationManager, logger)
    {
        _equipmentSheetService = equipmentSheetService;
        
        InitializeCommands();
    }
    
    #endregion
    
    #region Commands

    public AsyncDelegateCommand PivotGridLoadedCommand { [UsedImplicitly] get; set; } = null!;
    
    #endregion
    
    #region Command managment

    private void InitializeCommands()
    {
        PivotGridLoadedCommand = new AsyncDelegateCommand(OnPivotGridLoaded);
    }
    
    #endregion
    
    #region Private methods

    private async Task OnPivotGridLoaded()
    {
        Console.WriteLine("InitializePivotDataAsync called"); // для отладки
        var rows = await GetRowsAsync();
        var columns = await GetColumnsAsync();
        Console.WriteLine($"Rows count: {rows?.Count}"); // для отладки
        
        if (rows == null || rows.Count == 0)
        {
            CreateEmptyDataView();
            return;
        }
        
        var dt = new DataTable();
        
        foreach (var column in columns)
        {
            Console.WriteLine($"ColumnMappingName: {column.MappingName} HeaderText: {column.HeaderText}");
            dt.Columns.Add(column.MappingName, typeof(object));
        }
        
        foreach (var row in rows)
        {
            var newRow = dt.NewRow();
            foreach (var cellPair in row.CellsByMappingName)
            {
                if (dt.Columns.Contains(cellPair.Key))
                {
                    newRow[cellPair.Key] = cellPair.Value.Value;
                    Console.WriteLine($"Cell key (mappingName): {cellPair.Key} Value: {cellPair.Value.Value}");
                }
            }
            dt.Rows.Add(newRow);
        }

        ConfigurePivotFields(columns);
        
        PivotData = dt.DefaultView;
        _isInitialized = true;
    }
    
    private void ConfigurePivotFields(List<ColumnViewModel> columns)
    {
        PivotRows.Clear();
        PivotColumns.Clear();
        PivotCalculations.Clear();

        foreach (var column in columns)
        {
            var pivotItem = new PivotItem 
            { 
                FieldCaption = column.HeaderText, 
                FieldHeader = column.HeaderText,
                FieldMappingName = column.MappingName 
            };

            // Пример логики распределения полей
            if (IsNumericColumn(column))
            {
                PivotCalculations.Add(new PivotComputationInfo 
                { 
                    FieldName = column.MappingName, 
                    FieldCaption = column.HeaderText ,
                    FieldHeader = column.HeaderText,
                });
            }
            else if (IsDateColumn(column))
            {
                PivotColumns.Add(pivotItem);
            }
            else
            {
                PivotRows.Add(pivotItem);
            }
        }
    }
    
    private bool IsNumericColumn(ColumnViewModel column)
    {
        if (column.DataType is ColumnDataType.Currency or ColumnDataType.Number)
        {
            return true;
        }
        return false;
    }
    
    private bool IsDateColumn(ColumnViewModel column)
    {
        if (column.DataType is ColumnDataType.Date)
        {
            return true;
        }
        return false;
    }
    
    private void CreateEmptyDataView()
    {
        var emptyDt = new DataTable();
        emptyDt.Columns.Add("Message", typeof(string));
        var emptyRow = emptyDt.NewRow();
        emptyRow["Message"] = "Данные не доступны";
        emptyDt.Rows.Add(emptyRow);
        PivotData = emptyDt.DefaultView;
    }

    private async Task<List<RowViewModel>> GetRowsAsync()
    {
        var id = Guid.Parse("ab34de4f-d5ea-4c27-94ed-8b3b758c7008");
        var rowModels = await _equipmentSheetService.GetActiveRowsByEquipmentSheetIdAsync(id);
        Console.WriteLine("Row models count: " + rowModels.Count);
        return new List<RowViewModel>(
            rowModels
                .Select(RowViewModel.FromDomain));
    }

    private async Task<List<ColumnViewModel>> GetColumnsAsync()
    {
        /*var id = Guid.Parse("ab34de4f-d5ea-4c27-94ed-8b3b758c7008");
        var columnModels = new List<ColumnModel>(); //await _equipmentSheetService.GetActiveColumnsByEquipmentSheetIdAsync(id);*/

        var result = new List<ColumnViewModel>();
        /*foreach (var column in columnModels)
        {
            result.Add(new ColumnViewModel
            {
                HeaderText = column.HeaderText,
                MappingName = column.MappingName,
                DataType = column.DataType,
            });
        }*/
        
        return result;
    }
    
    #endregion

    #region Navigation 
    
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if(!_isInitialized)
         return;
        
        _pivotSheetId = navigationContext.Parameters.GetValue<Guid>("PivotSheetId");
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;
    public void OnNavigatedFrom(NavigationContext navigationContext) { }
    
    #endregion
}