using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Dynamic;
using Prism.Mvvm;
using Prism.Commands;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Services.DataGrid;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;

namespace UI.ViewModels.DataGrid
{
    public class SparePartsDataGridViewModel : BindableBase
    {
        private readonly IDataGridColumnService _dataGridColumnService;
        private Dictionary<string, string> _columnTypes;
        
        private readonly ISparePartsService _sparePartsService;
        private readonly IAppLogger<SparePartsDataGridViewModel> _logger;
        private readonly IEventAggregator _eventAggregator;
        
        private readonly DataGridViewModel _dataGridViewModel;
        
        private bool _isLoaded = false;
        private int _equipmentId;
        private string _currentSparePartsTableName;
        private CancellationTokenSource _cts;
        private SubscriptionToken _dataChangedToken;
        
        private ObservableCollection<ExpandoObject> _spareParts;
        private ExpandoObject _selectedSparePart;
        
        private SfDataGrid _sfDataGrid;
        private DelegateCommand<CurrentCellEndEditEventArgs> _currentCellEndEditCommand;
        private DelegateCommand _deleteRecordCommand;

        public ObservableCollection<ExpandoObject> SpareParts
        {
            get => _spareParts;
            set => SetProperty(ref _spareParts, value);
        }
        public ExpandoObject SelectedSparePart
        {
            get => _selectedSparePart;
            set => SetProperty(ref _selectedSparePart, value);
        }
        
        private DelegateCommand<SfDataGrid> _sparePartsLoadedCommand;
        
        public DelegateCommand<SfDataGrid> SparePartsLoadedCommand =>
            _sparePartsLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnSparePartsLoaded);

        public DelegateCommand<CurrentCellEndEditEventArgs> CurrentCellEndEditCommand =>
            _currentCellEndEditCommand ??= new DelegateCommand<CurrentCellEndEditEventArgs>(HandleCurrentCellEndEdit);
        
        public DelegateCommand DeleteRecordCommand =>
            _deleteRecordCommand ??= new DelegateCommand(OnDeleteRecord);

        public SparePartsDataGridViewModel(
            IDataGridColumnService dataGridColumnService, 
            ISparePartsService sparePartsService, 
            IAppLogger<SparePartsDataGridViewModel> logger,
            IEventAggregator eventAggregator)
        {
            _dataGridColumnService = dataGridColumnService;
            _sparePartsService = sparePartsService;
            _logger = logger;
            _eventAggregator = eventAggregator;
            
            _spareParts = new ObservableCollection<ExpandoObject>();
            
            _cts = new CancellationTokenSource();
            _dataChangedToken = _eventAggregator.GetEvent<DataChangedEvent>()
                .Subscribe(OnDataChanged, ThreadOption.UIThread);
        }

        #region SpareParts_CollectionChanged
        private async void SpareParts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            try
            {
                // Add new record
                if (args.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in args.NewItems)
                    {
                        var record = item as IDictionary<string, object>;
                        if (record != null)
                        {
                            _logger.LogInformation("Adding new record to table {TableName}", _currentSparePartsTableName);
                            var dict = new Dictionary<string, object>();
                            foreach (var prop in record)
                            {
                                dict[prop.Key] = prop.Value;
                            }
                            var insertedId = await _sparePartsService.InsertRecordAsync(_currentSparePartsTableName, _equipmentId, dict);
                            if (insertedId != null)
                            {
                                record["id"] = insertedId;
                                _logger.LogInformation("Added record with ID {Id} to table {TableName}", insertedId, _currentSparePartsTableName);
                            }
                            else
                            {
                                _logger.LogWarning("Failed to retrieve ID for new record in table {TableName}", _currentSparePartsTableName);
                            }
                        }
                    }
                }

                // Delete record
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    foreach (var item in args.OldItems)
                    {
                        var record = item as IDictionary<string, object>;
                        if (record != null && record.ContainsKey("id") && record["id"] != null)
                        {
                            var id = record["id"];
                            await _sparePartsService.DeleteRecordAsync(_currentSparePartsTableName, id);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
        }
        #endregion
        
        #region OnDeleteRecord
        private void OnDeleteRecord()
        {
            try
            {
                if (SelectedSparePart != null)
                {
                    SpareParts.Remove(SelectedSparePart);
                }
                else
                {
                    _logger.LogWarning("No record selected for deletion in table {TableName}", _currentSparePartsTableName);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                throw;
            }
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
                    _logger.LogWarning("No row data found for cell edit at row {RowIndex}, column {ColumnIndex} in table {TableName}",
                        rowIndex, mappingName, _currentSparePartsTableName);
                    return;
                }

                if (!rowData.ContainsKey("id"))
                {
                    _logger.LogInformation("No ID found, assuming new record creation in table {TableName}", _currentSparePartsTableName);
                    return;
                }

                var id = rowData["id"];
                var newValue = rowData[mappingName];

                var updateDic = new Dictionary<string, object> { { mappingName, newValue } };
                await _sparePartsService.UpdateRecordAsync(_currentSparePartsTableName, id, updateDic);
                _logger.LogInformation("Successfully updated field {FieldName} for record ID {Id} in table {TableName}",
                    mappingName, id, _currentSparePartsTableName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cell in table {TableName})", _currentSparePartsTableName);
                throw;
            }
        }
        #endregion
        
        private async void OnDataChanged(string payload)
        {
            if (payload.Contains(_currentSparePartsTableName, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Data changed for table {TableName}: {Payload}", _currentSparePartsTableName, payload);
                _isLoaded = false;
                await LoadData();
            }
        }
        
        #region OnSparePartsLoaded
        private async void OnSparePartsLoaded(SfDataGrid sfDataGrid)
        {
            _sfDataGrid = sfDataGrid;
            _equipmentId = SparePartsData.EquipmentId;
            _currentSparePartsTableName = SparePartsData.SparePartsTableName;
            
            try
            {
                if (_columnTypes == null)
                {
                    _columnTypes = await _dataGridColumnService.GetColumnTypesAsync(_currentSparePartsTableName);
                    _logger.LogInformation("Loaded Columns {Count}, from table {TableName}", _columnTypes.Count, _currentSparePartsTableName);
                }
            
                _sfDataGrid.Columns.Clear();
                foreach (var columnInfo in _columnTypes)
                {
                    var column = _dataGridColumnService.CreateColumnFromDbType(columnInfo.Key, columnInfo.Value);
                    _sfDataGrid.Columns.Add(column);
                }
                
                await LoadData();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
        #endregion

        #region LoadData
        private async Task LoadData()
        {
            if (_isLoaded == false)
            {
                try
                {
                    SpareParts.CollectionChanged -= SpareParts_CollectionChanged;
                    SpareParts.Clear();
                    var data = await _sparePartsService.GetDataAsync(_currentSparePartsTableName, _equipmentId);
                    foreach (var part in data)
                    {
                        SpareParts.Add(part);
                    }

                    SpareParts.CollectionChanged += SpareParts_CollectionChanged;
                    _isLoaded = true;
                    _logger.LogInformation("Loaded {Count} record for spare parts", data.Count);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message);
                }
            }
        }
        #endregion
    }
}

