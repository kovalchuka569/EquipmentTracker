using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.ComponentModel;
using System.Drawing.Printing;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid.Converter;

using Core.Services.DataGrid;
using Core.Models.DataGrid;

using Common.Logging;
using Core.Events.DataGrid;
using Prism.Common;
using Prism.Events;
using Syncfusion.Windows.Controls.Input;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase, INavigationAware
{
    
    private readonly IAppLogger<DataGridViewModel> _logger;
    private readonly IDataGridService _dataGridService;
    private readonly IDataGridColumnService _dataGridColumnService;
    private readonly IEventAggregator _eventAggregator;
    
    private string _currentTableName;
    private bool _isLoaded = false;
    private CancellationTokenSource _cts;
    private SubscriptionToken _dataChangedToken;
    
    private ObservableCollection<ExpandoObject> _items;
    private ExpandoObject _selectedItem;
    
    private ObservableCollection<ExpandoObject> _spareParts;
    private ExpandoObject _selectedSparePart;
    
    private Dictionary<string, string> _columnTypes;
    private Dictionary<string, string> _columnTypesSpareParts;
    
    private string _guid = Guid.NewGuid().ToString();
    
    private SfDataGrid _sfDataGrid;
    private SfDataGrid _sparePartsDataGrid;
    
    private DelegateCommand<CurrentCellBeginEditEventArgs> _cellBeginEditCommand;
    private DelegateCommand<CurrentCellEndEditEventArgs> _currentCellEndEditCommand;
    private DelegateCommand<GridDetailsViewExpandingEventArgs> _detailsViewExpandingCommand;
    private DelegateCommand<SfDataGrid> _sfDataGridLoadedCommand;
    private DelegateCommand<SfDataGrid> _sparePartsDataGridLoadedCommand;
    private DelegateCommand _deleteRecordCommand;
    private DelegateCommand _printCommand;
    private DelegateCommand _excelExportCommand;
    
    public ObservableCollection<ExpandoObject> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ExpandoObject SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    
    public DelegateCommand<CurrentCellBeginEditEventArgs> CellBeginEditCommand =>
        _cellBeginEditCommand ??= new DelegateCommand<CurrentCellBeginEditEventArgs>(HandleCellBeginEdit);

    public DelegateCommand<CurrentCellEndEditEventArgs> CurrentCellEndEditCommand =>
        _currentCellEndEditCommand ??= new DelegateCommand<CurrentCellEndEditEventArgs>(HandleCurrentCellEndEdit);

    public DelegateCommand<GridDetailsViewExpandingEventArgs> DetailsViewExpandingCommand =>
        _detailsViewExpandingCommand ??= new DelegateCommand<GridDetailsViewExpandingEventArgs>(OnDetailsViewExpanding);

    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand =>
        _sfDataGridLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
    
    public DelegateCommand DeleteRecordCommand =>
        _deleteRecordCommand ??= new DelegateCommand(OnDeleteRecord);

    public DelegateCommand PrintCommand =>
        _printCommand ??= new DelegateCommand(OnPrint);
    public DelegateCommand ExcelExportCommand =>
        _excelExportCommand ??= new DelegateCommand(OnExcelExport);
    
    
    
    public DataGridViewModel(IAppLogger<DataGridViewModel> logger, 
        IDataGridService dataGridService, 
        IDataGridColumnService dataGridColumnService,
        IEventAggregator eventAggregator)
    {
        _logger = logger;
        _dataGridService = dataGridService;
        _dataGridColumnService = dataGridColumnService;
        _eventAggregator = eventAggregator;
        
        _items = new ObservableCollection<ExpandoObject>();
    }
    

    #region Items_CollectionChanged
    private async void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        try
        {
            // Add new record
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var record = item as IDictionary<string, object>;
                    if (record != null)
                    {
                        _logger.LogInformation("Adding new record to table {TableName} (GUID: {Guid})", _currentTableName, _guid);
                        var dict = new Dictionary<string, object>();
                        foreach (var prop in record)
                        {
                            dict[prop.Key] = prop.Value;
                        }
                        var insertedId = await _dataGridService.InsertRecordAsync(_currentTableName, dict);
                        if (insertedId != null)
                        {
                            record["id"] = insertedId;
                            _logger.LogInformation("Added record with ID {Id} to table {TableName} (GUID: {Guid})", insertedId, _currentTableName, _guid);
                        }
                        else
                        {
                            _logger.LogWarning("Failed to retrieve ID for new record in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
                        }
                    }
                }
            }
            
            // Delete record
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var record = item as IDictionary<string, object>;
                    if (record != null && record.ContainsKey("id") && record["id"] != null)
                    {
                        var id = record["id"];
                        await _dataGridService.DeleteRecordAsync(_currentTableName, id);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during collection change for table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            throw;
        }
    }
    #endregion

    #region OnDeleteRecord
    private void OnDeleteRecord()
    {
        try
        {
            if (SelectedItem != null)
            {
                Items.Remove(SelectedItem);
                _logger.LogInformation("Selected record deleted from table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            }
            else
            {
                _logger.LogWarning("No record selected for deletion in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting record in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            throw;
        }
    }
    #endregion
    
    #region HandleCellBeginEdit
    private void HandleCellBeginEdit(CurrentCellBeginEditEventArgs args)
    {
        _logger.LogInformation("Starting cell edit in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
    }
    #endregion
    
    #region HandleCurrentCellEndEdit
    private async void HandleCurrentCellEndEdit(CurrentCellEndEditEventArgs args)
    {
        try
        {
            var rowIndex = args.RowColumnIndex.RowIndex;
            var currentColumn = _sfDataGrid.CurrentColumn;
            var mappingName = currentColumn.MappingName;
            
            _logger.LogInformation("Edited column name: {MappingName}", mappingName);
            
            var rowData = _sfDataGrid.GetRecordAtRowIndex(rowIndex) as IDictionary<string, object>;

            if (rowData == null)
            {
                _logger.LogWarning("No row data found for cell edit at row {RowIndex}, column {ColumnIndex} in table {TableName} (GUID: {Guid})",
                    rowIndex, mappingName, _currentTableName, _guid);
                return;
            }

            if (!rowData.ContainsKey("id"))
            {
                _logger.LogInformation("No ID found, assuming new record creation in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
                return;
            }

            var id = rowData["id"];
            var newValue = rowData[mappingName];

            var updateDic = new Dictionary<string, object> { { mappingName, newValue } };
            await _dataGridService.UpdateRecordAsync(_currentTableName, id, updateDic);
            _logger.LogInformation("Successfully updated field {FieldName} for record ID {Id} in table {TableName} (GUID: {Guid})",
                mappingName, id, _currentTableName, _guid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cell in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            throw;
        }
    }
    #endregion
    

    #region OnDataGridLoaded
    private async void OnDataGridLoaded(SfDataGrid sfDataGrid)
    {
            _sfDataGrid = sfDataGrid;
            _logger.LogInformation("DataGrid loaded for table {TableName} (GUID: {Guid})", _currentTableName, _guid);

            if (_columnTypes == null)
            {
                _columnTypes = await _dataGridColumnService.GetColumnTypesAsync(_currentTableName);
            }
        
            _sfDataGrid.Columns.Clear();
            foreach (var columnInfo in _columnTypes)
            {
                var column = _dataGridColumnService.CreateColumnFromDbType(columnInfo.Key, columnInfo.Value);
                _sfDataGrid.Columns.Add(column);
            }
        
            await LoadData();
        
    }
    #endregion
    
    #region LoadData
    private async Task LoadData()
    {
        if (_isLoaded == false)
        {
            try
            {
                _logger.LogInformation("Loading data for table {TableName}", _currentTableName);
                Items.CollectionChanged -= Items_CollectionChanged;
                Items.Clear();
                var data = await _dataGridService.GetDataAsync(_currentTableName);
                foreach (var item in data)
                {
                    Items.Add(item);
                }
                Items.CollectionChanged += Items_CollectionChanged;
                
                _isLoaded = true;

                _logger.LogInformation("Successfully loaded {Count} records for table {TableName}", data.Count,
                    _currentTableName);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to load data for table {TableName}", _currentTableName);
                throw;
            }
        }
    }
    #endregion
    
    // Load data when details view expanding
    private void OnDetailsViewExpanding(GridDetailsViewExpandingEventArgs args)
    {

        string currentSparePartsTableName = $"{_currentTableName} ЗЧ";
    
        if (args.Record is ExpandoObject expando && expando is IDictionary<string, object> dictionary &&
            dictionary.TryGetValue("id", out var idObj) && idObj is int id)
        {

            SparePartsData.EquipmentId = id;
            SparePartsData.TableName = _currentTableName;
            SparePartsData.SparePartsTableName = currentSparePartsTableName;
            
        }
    }

    #region OnPrint

    private void OnPrint()
    {
        _sfDataGrid.ShowPrintPreview();
        _sfDataGrid.PrintSettings.AllowPrintStyles = true;
    }

    #endregion

    private void OnExcelExport()
    {
        var options = new ExcelExportingOptions();
        options.ExportMode = ExportMode.Text;
        var excelEngine = _sfDataGrid.ExportToExcel(_sfDataGrid.View, options);
        var workBook = excelEngine.Excel.Workbooks[0];
        workBook.SaveAs("Sample.xlsx");
    }

    
    
    #region Navigation
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        _currentTableName = navigationContext.Parameters["TableName"] as string;
    }
    
    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _logger.LogInformation("Navigated away from DataGridViewModel for table {TableName} (GUID: {Guid})", _currentTableName, _guid);
        _eventAggregator.GetEvent<DataChangedEvent>().Unsubscribe(_dataChangedToken);
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        Items.Clear();
        _logger.LogInformation("Navigated away from table {TableName}", _currentTableName);
    }
    #endregion
    
    
    
    
}
