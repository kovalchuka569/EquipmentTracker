using System.Collections.ObjectModel;
using System.Dynamic;
using Core.Events.DataGrid;
using Core.Services.DataGrid;
using Syncfusion.UI.Xaml.Grid;

namespace UI.ViewModels.DataGrid
{
    public class SparePartsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IDataGridColumnService _dataGridColumnService;
        private Dictionary<string, string> _columnTypes;
        
        private readonly ISparePartsService _sparePartsService;
        private readonly IEventAggregator _eventAggregator;
        
        private readonly DataGridViewModel _dataGridViewModel;
        
        private bool _isLoaded = false;
        private int _equipmentId;
        private string _currentSparePartsTableName;
        
        private ObservableCollection<ExpandoObject> _spareParts;
        private ExpandoObject _selectedSparePart;
        private ObservableCollection<string> _measurementUnits;
        
        private SfDataGrid _sfDataGrid;

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
        public ObservableCollection<string> MeasurementUnits
        {
            get => _measurementUnits;
            set => SetProperty(ref _measurementUnits, value);
        }
        
        private DelegateCommand<SfDataGrid> _sparePartsLoadedCommand;
        
        public DelegateCommand<SfDataGrid> SparePartsLoadedCommand =>
            _sparePartsLoadedCommand ??= new DelegateCommand<SfDataGrid>(OnSparePartsLoaded);

        public SparePartsDataGridViewModel(IDataGridColumnService dataGridColumnService, ISparePartsService sparePartsService, IEventAggregator eventAggregator)
        {
            Console.WriteLine("SparePartsDataGridViewModel");
            _dataGridColumnService = dataGridColumnService;
            _sparePartsService = sparePartsService;
            _eventAggregator = eventAggregator;
            
            _spareParts = new ObservableCollection<ExpandoObject>();
        }
        
        
        

        private async void OnSparePartsLoaded(SfDataGrid sfDataGrid)
        {
            _equipmentId = SparePartsData.EquipmentId;
            _currentSparePartsTableName = SparePartsData.SparePartsTableName;
            
            try
            {
                _sfDataGrid = sfDataGrid;
                _isLoaded = true;
        
                Console.WriteLine("OnSparePartsLoaded");
                Console.WriteLine($"Использую сохраненные значения: ID={_equipmentId}, Таблица={_currentSparePartsTableName}");
                
                if (_columnTypes == null)
                {
                    _columnTypes = await _dataGridColumnService.GetColumnTypesAsync(_currentSparePartsTableName);
                    Console.WriteLine($"Получено {_columnTypes.Count} типов колонок");
                }

                _sfDataGrid.Columns.Clear();
                foreach (var columnInfo in _columnTypes)
                {
                    var column = _dataGridColumnService.CreateColumnFromDbType(columnInfo.Key, columnInfo.Value);
                    _sfDataGrid.Columns.Add(column);
                }
                
                await LoadData();
        
                Console.WriteLine($"Загружено {SpareParts.Count} записей");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в OnSparePartsLoaded: {ex.Message}");
            }
        }

        private async Task LoadData()
        {
            try
            {
                SpareParts.Clear();
                var data = await _sparePartsService.GetDataAsync(_currentSparePartsTableName, _equipmentId);
                foreach (var part in data)
                {
                    SpareParts.Add(part);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void CreateMeasurementUnits()
        {
            _measurementUnits = new ObservableCollection<string>
            {
                "шт",
                "кг",
                "м",
                "л",
                "см",
                "мм",
                "м²",
                "м³",
                "компл",
                "пара"
            };
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Console.WriteLine("OnNavigatedTo SparePartsDataGridViewModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => false;

        public void OnNavigatedFrom(NavigationContext navigationContext) {}
    }
}

