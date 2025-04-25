using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

using Core.Services.DataGrid;
using Core.Models.DataGrid;

using Common.Logging;
using Core.Events.DataGrid;
using Prism.Common;
using Syncfusion.Windows.Controls.Input;

namespace UI.ViewModels.DataGrid;

public class DataGridViewModel : BindableBase, INavigationAware
{
    
    private readonly IAppLogger<DataGridViewModel> _logger;
    private readonly IEventAggregator _eventAggregator;
    private readonly IDataGridService _dataGridService;
    private readonly IDataGridColumnService _dataGridColumnService;
    private readonly ISparePartsService _sparePartsService;
    
    private string _currentTableName;
    private string _currentSparePartsTableName;
    private bool _isLoaded = false;
    private bool _isDataLoading = false;
    
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
    private AsyncDelegateCommand<CurrentCellEndEditEventArgs> _currentCellEndEditCommand;
    private DelegateCommand<RowValidatedEventArgs> _rowValidatedCommand;
    private DelegateCommand<GridDetailsViewExpandingEventArgs> _detailsViewLoadingCommand;
    private DelegateCommand<GridDetailsViewExpandedEventArgs> _detailsViewExpandedCommand;
    private DelegateCommand _testCommand;
    
    private DelegateCommand<SfDataGrid> _sfDataGridLoadedCommand;
    private DelegateCommand<SfDataGrid> _sparePartsDataGridLoadedCommand;
    
    private DelegateCommand _deleteRecordCommand;
    
    public ObservableCollection<ExpandoObject> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }
    public ObservableCollection<ExpandoObject> SpareParts
    {
        get => _spareParts;
        set => SetProperty(ref _spareParts, value);
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

    public DelegateCommand<GridDetailsViewExpandingEventArgs> DetailsViewLoadingCommand =>
        _detailsViewLoadingCommand ??= new DelegateCommand<GridDetailsViewExpandingEventArgs>(OnDetailsViewExpanding);

    public DelegateCommand<GridDetailsViewExpandedEventArgs> DetailsViewExpandedCommand =>
        _detailsViewExpandedCommand ??= new DelegateCommand<GridDetailsViewExpandedEventArgs>(OnDetailsViewExpanded);

    public DelegateCommand<SfDataGrid> SfDataGridLoadedCommand =>
        _sfDataGridLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnDataGridLoaded);

    public DelegateCommand<SfDataGrid> SparePartsDataGridLoadedCommand =>
        _sparePartsDataGridLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnSparePartsDataGridLoaded);
    
    public DelegateCommand DeleteRecordCommand =>
        _deleteRecordCommand ??= new DelegateCommand(OnDeleteRecord);
    
    
    
    public DataGridViewModel(IAppLogger<DataGridViewModel> logger, 
        IDataGridService dataGridService, 
        IDataGridColumnService dataGridColumnService, 
        ISparePartsService sparePartsService,
        IEventAggregator eventAggregator)
    {
        Console.WriteLine("DataGridViewModel");
        _logger = logger;
        _dataGridService = dataGridService;
        _dataGridColumnService = dataGridColumnService;
        _sparePartsService = sparePartsService;
        _eventAggregator = eventAggregator;
        
        _items = new ObservableCollection<ExpandoObject>();
        
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
        if (_isLoaded == false)
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
            
            _isLoaded = true;
        }
    }
    #endregion
    
    #region LoadData
    private async Task LoadData()
    {
        if(_isDataLoading) return;
        try
        {
            _isDataLoading = true;
            _logger.LogInformation("Loading data for table {TableName}", _currentTableName);
            Items.CollectionChanged -= Items_CollectionChanged;
            Items.Clear();
            var data = await _dataGridService.GetDataAsync(_currentTableName);
            foreach (var item in data)
            {
                Items.Add(item);
            }

            _logger.LogInformation("Successfully loaded {Count} records for table {TableName}", data.Count,
                _currentTableName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to load data for table {TableName}", _currentTableName);
            throw;
        }
        finally
        {
            Items.CollectionChanged += Items_CollectionChanged;
            _isDataLoading = false;
        }
    }
    #endregion
    
    // Load data when details view expanding
    private void OnDetailsViewExpanding(GridDetailsViewExpandingEventArgs args)
    {
        Console.WriteLine("OnDetailsViewExpanding");

        string currentSparePartsTableName = $"{_currentTableName} запасні частини";
    
        if (args.Record is ExpandoObject expando && expando is IDictionary<string, object> dictionary &&
            dictionary.TryGetValue("id", out var idObj) && idObj is int id)
        {

            SparePartsData.EquipmentId = id;
            SparePartsData.TableName = _currentTableName;
            SparePartsData.SparePartsTableName = currentSparePartsTableName;
            
            Console.WriteLine($"Сохранено: ID={id}, TableName={_currentTableName}");
        }
    }
    
    // Assign columns to spare parts
    private async void OnSparePartsDataGridLoaded(SfDataGrid sparePartsDataGrid)
    {
        try
        {
            Console.WriteLine("OnSparePartsDataGridLoaded начал выполнение");
            Console.WriteLine($"sparePartsDataGrid.DataContext: {sparePartsDataGrid.DataContext}");
            
            // Запоминаем ссылку на grid для использования в других методах
            _sparePartsDataGrid = sparePartsDataGrid;
            
            // Получаем RecordEntry и извлекаем из него оригинальную запись
            if (sparePartsDataGrid.DataContext is Syncfusion.Data.RecordEntry recordEntry)
            {
                Console.WriteLine("DataContext это RecordEntry, получаем оригинальную запись");
                
                // Выводим все public-свойства RecordEntry
                var recordEntryProperties = recordEntry.GetType().GetProperties();
                Console.WriteLine($"RecordEntry имеет {recordEntryProperties.Length} свойств:");
                foreach (var prop in recordEntryProperties)
                {
                    try
                    {
                        var value = prop.GetValue(recordEntry);
                        Console.WriteLine($"  Свойство {prop.Name}: {value}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"  Ошибка чтения свойства {prop.Name}: {ex.Message}");
                    }
                }
                
                // Пытаемся использовать нетипизированный индексер для получения данных
                try 
                {
                    // Используем рефлексию для получения индексера
                    var itemProperty = recordEntry.GetType().GetProperty("Item");
                    if (itemProperty != null)
                    {
                        Console.WriteLine("RecordEntry имеет индексер Item. Пытаемся получить SpareParts");
                        // Получаем значение по ключу "SpareParts"
                        var spareParts = itemProperty.GetValue(recordEntry, new object[] { "SpareParts" });
                        
                        if (spareParts != null)
                        {
                            Console.WriteLine($"Получено значение SpareParts: {spareParts}, тип: {spareParts.GetType().Name}");
                            if (spareParts is ObservableCollection<ExpandoObject> collection)
                            {
                                sparePartsDataGrid.ItemsSource = collection;
                                Console.WriteLine($"ItemsSource установлен из Item[SpareParts], количество: {collection.Count}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при попытке получить данные через индексер: {ex.Message}");
                }
                
                // Прямой доступ к DataContext нижележащего Grid
                try
                {
                    var currentItem = _sfDataGrid.SelectedItem as ExpandoObject;
                    if (currentItem != null)
                    {
                        var dict = currentItem as IDictionary<string, object>;
                        if (dict.ContainsKey("SpareParts") && dict["SpareParts"] is ObservableCollection<ExpandoObject> parts)
                        {
                            sparePartsDataGrid.ItemsSource = parts;
                            Console.WriteLine($"ItemsSource установлен из SelectedItem[SpareParts], количество: {parts.Count}");
                        }
                        else
                        {
                            Console.WriteLine("SelectedItem не содержит SpareParts");
                        }
                    }
                    else
                    {
                        Console.WriteLine("SelectedItem равен null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при попытке установить ItemsSource напрямую: {ex.Message}");
                }
            }
            
            // Получаем типы колонок
            if (_columnTypesSpareParts == null)
            {
                _columnTypesSpareParts = await _dataGridColumnService.GetColumnTypesAsync(_currentSparePartsTableName);
                Console.WriteLine($"Загружено {_columnTypesSpareParts.Count} типов колонок для {_currentSparePartsTableName}");
            }
            
            // Очищаем колонки и настраиваем
            sparePartsDataGrid.Columns.Clear();
            foreach (var columnInfo in _columnTypesSpareParts)
            {
                var column = _dataGridColumnService.CreateColumnFromDbType(columnInfo.Key, columnInfo.Value);
                sparePartsDataGrid.Columns.Add(column);
                Console.WriteLine($"Добавлена колонка: {columnInfo.Key}, тип: {columnInfo.Value}");
            }
            
            Console.WriteLine($"ItemsSource текущий: {sparePartsDataGrid.ItemsSource != null}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в OnSparePartsDataGridLoaded: {ex.Message}");
        }
    }
    
    #region Navigation
    public async void OnNavigatedTo(NavigationContext navigationContext)
    {
        try
        {
            _currentTableName = navigationContext.Parameters["parameter"] as string;
            _currentSparePartsTableName = $"{_currentTableName} запасні частини";
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
    
    

    private async void OnDetailsViewExpanded(GridDetailsViewExpandedEventArgs args)
    {
        /*string sparePartsTableName = $"{_currentTableName} запасні частини";
       _columnTypesSpareParts = await _dataGridColumnService.GetColumnTypesAsync(sparePartsTableName);
       _sparePartsDataGrid.Columns.Clear();
       foreach (var columnInfo in _columnTypesSpareParts)
       {
           var column = _dataGridColumnService.CreateColumnFromDbType(columnInfo.Key, columnInfo.Value);
           _sparePartsDataGrid.Columns.Add(column);
           Console.WriteLine($"Loaded {columnInfo.Key}: {columnInfo.Value}");
       }*/
    }
    
}
