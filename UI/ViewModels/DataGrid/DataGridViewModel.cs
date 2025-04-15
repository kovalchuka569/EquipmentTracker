using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using System.Windows.Controls;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

using Core.Services.DataGrid;
using Core.Models.DataGrid;

using Common.Logging;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase, INavigationAware
{
    
    private readonly IAppLogger<DataGridViewModel> _logger;
    private readonly IDataGridService _dataGridService;
    private readonly IDataGridColumnService _dataGridColumnService;
    
    private string _currentTableName;
    private ObservableCollection<ExpandoObject> _data;
    private Dictionary<string, string> _columnTypes;
    private ExpandoObject _selectedItem;
    private string _guid = Guid.NewGuid().ToString();
    private SfDataGrid _sfDataGrid;
    
    private DelegateCommand<CurrentCellBeginEditEventArgs> _cellBeginEditCommand;
    private AsyncDelegateCommand<CurrentCellEndEditEventArgs> _currentCellEndEditCommand;
    private DelegateCommand<RowValidatedEventArgs> _rowValidatedCommand;
    private DelegateCommand<SfDataGrid> _sfDataGridLoadedCommand;
    private DelegateCommand _deleteRecordCommand;
    
    public ObservableCollection<ExpandoObject> Items
    {
        get => _data;
        set => SetProperty(ref _data, value);
    }

    public ExpandoObject SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    
    public DelegateCommand<CurrentCellBeginEditEventArgs> CellBeginEditCommand =>
        _cellBeginEditCommand ??= new DelegateCommand<CurrentCellBeginEditEventArgs>(HandleCellBeginEdit);

    public AsyncDelegateCommand<CurrentCellEndEditEventArgs> CurrentCellEndEditCommand =>
        _currentCellEndEditCommand ??= new AsyncDelegateCommand<CurrentCellEndEditEventArgs>(HandleCurrentCellEndEdit);
    public DelegateCommand<RowValidatedEventArgs> RowValidatedCommand =>
    _rowValidatedCommand??= new DelegateCommand<RowValidatedEventArgs>(HandleRowValidated);

    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand =>
        _sfDataGridLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
    public DelegateCommand DeleteRecordCommand =>
        _deleteRecordCommand ??= new DelegateCommand(OnDeleteRecord);
    
    
    public DataGridViewModel(IAppLogger<DataGridViewModel> logger, IDataGridService dataGridService, IDataGridColumnService dataGridColumnService)
    {
        _logger = logger;
        _dataGridService = dataGridService;
        _dataGridColumnService = dataGridColumnService;
        _data = new ObservableCollection<ExpandoObject>();
        
        _logger.LogInformation("DataGridViewModel loaded");
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
                        _logger.LogInformation("Removing record with ID {Id} from table {TableName} (GUID: {Guid})", id, _currentTableName, _guid);
                        await _dataGridService.DeleteRecordAsync(_currentTableName, id);
                        _logger.LogInformation("Removed record with ID {Id} from table {TableName} (GUID: {Guid})", id, _currentTableName, _guid);
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
                _logger.LogInformation("Initiating delete of selected record in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
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
    private async Task HandleCurrentCellEndEdit(CurrentCellEndEditEventArgs args)
    {
        try
        {
            var rowIndex = args.RowColumnIndex.RowIndex;
            var columnIndex = args.RowColumnIndex.ColumnIndex;
            var mappingName = _sfDataGrid.Columns[columnIndex].MappingName;
            var rowData = _sfDataGrid.GetRecordAtRowIndex(rowIndex) as IDictionary<string, object>;

            if (rowData == null)
            {
                _logger.LogWarning("No row data found for cell edit at row {RowIndex}, column {ColumnIndex} in table {TableName} (GUID: {Guid})",
                    rowIndex, columnIndex, _currentTableName, _guid);
                return;
            }

            if (!rowData.ContainsKey("id"))
            {
                _logger.LogInformation("No ID found, assuming new record creation in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
                return;
            }

            var id = rowData["id"];
            var newValue = rowData[mappingName];
            _logger.LogInformation("Updating field {FieldName} with value {NewValue} for record ID {Id} in table {TableName} (GUID: {Guid})",
                mappingName, newValue, id, _currentTableName, _guid);

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

    #region HandleRowValidated
    private void HandleRowValidated(RowValidatedEventArgs args)
    {
        _logger.LogInformation("Row validated in table {TableName} (GUID: {Guid})", _currentTableName, _guid);
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
            _logger.LogInformation("Successfully loaded {Count} records for table {TableName}", data.Count, _currentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load data for table {TableName}", _currentTableName);
            throw;
        }
    }
    #endregion
    
    #region Navigation
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            _currentTableName = navigationContext.Parameters["parameter"] as string;
            _logger.LogInformation("Navigated to DataGridViewModel with table {TableName} (GUID: {Guid})", _currentTableName, _guid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during navigation to table {TableName} (GUID: {Guid})", _currentTableName, _guid);
            throw;
        }
    }
    
    public bool IsNavigationTarget(NavigationContext navigationContext) => false;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        _logger.LogInformation("Navigated away from DataGridViewModel for table {TableName} (GUID: {Guid})", _currentTableName, _guid);
    }
    #endregion
}
