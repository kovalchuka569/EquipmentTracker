using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Dynamic;
using System.Threading.Tasks;
using Core.Events.DataGrid.Consumables;
using Core.Services.Consumables.Operations;
using Core.Services.DataGrid;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls;
using UI.ViewModels.TabControl;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class OperationsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IOperationsDataGridService _service;
        private readonly IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;

        private ObservableCollection<dynamic> _operations = new();
        public ObservableCollection<dynamic> Operations
        {
            get => _operations;
            set => SetProperty(ref _operations, value);
        }

        private bool _isDataLoaded;
        private bool _isAddingNewOperation = false;
        private bool _isOverlayVisible = false;
        private int _materialId;
        private string _tableName;

        private bool _nullOperationsTipVisibility;
        private SfDataGrid _operationsDataGrid;

        public bool NullOperationsTipVisibility
        {
            get => _nullOperationsTipVisibility;
            set => SetProperty(ref _nullOperationsTipVisibility, value);
        }

        public bool IsOverlayVisible
        {
            get => _isOverlayVisible;
            set => SetProperty(ref _isOverlayVisible, value);
        }

        private const string WriteOffOperation = "Списання";
        private const string IncomeOperation = "Прихід";

        public DelegateCommand<SfDataGrid> DataGridLoadedCommand { get; }
        public DelegateCommand ShowAddNewTemplateCommand { get; }

        public OperationsDataGridViewModel(
            IOperationsDataGridService service,
            IEventAggregator eventAggregator, 
            IRegionManager regionManager)
        {
            _service = service;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
            ShowAddNewTemplateCommand = new DelegateCommand(OnShowAddNewTemplate);
        }

        private void OnShowAddNewTemplate()
        {
            _isAddingNewOperation = true;
            IsOverlayVisible = true;
            _regionManager.RequestNavigate("AddNewOperationTemplateRegion", "AddNewOperationView");
            _eventAggregator.GetEvent<CloseAddNewTemplateEvent>().Subscribe(OnCloseAddNewTemplate);
            _eventAggregator.GetEvent<AddNewOperationEvent>().Subscribe(OnAddNewOperation);
        }

        private void OnCloseAddNewTemplate()
        {
            _regionManager.Regions["AddNewOperationTemplateRegion"].RemoveAll();
            _eventAggregator.GetEvent<CloseAddNewTemplateEvent>().Unsubscribe(OnCloseAddNewTemplate);
            _isAddingNewOperation = false;
            IsOverlayVisible = false;
        }

        private async void OnAddNewOperation(AddNewOperationEventArgs args)
        {
           await _service.InsertRecordAsync(_tableName, _materialId, args.OperationType, args.DateTime.ToString(), args.Quantity, args.Description, args.User);
            _eventAggregator.GetEvent<AddNewOperationEvent>().Unsubscribe(OnAddNewOperation);
            _isDataLoaded = false;
            await LoadData();
        }

        private async void OnDataGridLoaded(SfDataGrid dataGrid)
        {
            _operationsDataGrid = dataGrid;
        }

        private async Task LoadData()
        {
            if (_isDataLoaded || _isAddingNewOperation)
            {
                return;
            }

            try
            {
                var data = await _service.GetDataAsync(_tableName, _materialId);
                
                if (data.Count == 0) NullOperationsTipVisibility = true;
                if(data.Count > 0) NullOperationsTipVisibility = false;
                    
                    Operations.Clear();

                    foreach (var item in data)
                    {
                        if (item is not IDictionary<string, object?> dictionary)
                            continue;
                        
                        string operationType = dictionary.TryGetValue("Тип операції", out var opType) ? opType?.ToString() : null;
                        string rawValue = dictionary.TryGetValue("Кількість", out var rwValue) ? rwValue?.ToString() : null;
                        string descriptionText = dictionary.TryGetValue("Опис", out var descriptionValue) ? descriptionValue?.ToString() : null;
                        
                        string worker = dictionary.TryGetValue("Користувач", out var workerValue) ? workerValue?.ToString() : null;

                        if (!double.TryParse(rawValue, out var numericValue))
                            continue;

                        dynamic operation = new ExpandoObject();
                        var operationDict = (IDictionary<string, object>)operation;
                        
                        operation.Quantity = numericValue;
                        operation.QuantityDisplay = GetFormattedValue(operationType, numericValue); 
                        operation.OperationForeground = GetOperationColor(operationType);
                        operation.OperationType = operationType;
                        operation.OperationTypeDisplay = operationType;
                        operation.CellBackground = GetCellBackgroundColor(operationType);
                    
                        operation.Description = descriptionText;
                        operation.DescriptionDisplay = descriptionText;
                    
                        DateTime? dateTime = dictionary.TryGetValue("Дата, час", out var dateTimeValues) ? dateTimeValues as DateTime? : null;
                        operationDict["DateTime"] = dateTime ?? default(DateTime);
                        operationDict["DateTimeDisplay"] = dateTime?.ToString("dd.MM.yyyy HH:mm");
                    
                        operation.Worker = worker;
                        operation.WorkerDisplay = worker;
                    

                        Operations.Add(operation);
                    }
            }
            catch
            {
                _isDataLoaded = false;
                throw;
            }
            finally
            {
                _isDataLoaded = true;
            }
        }

        private async void UpdateOperationsConsumablesDataGrid(UpdateOperationsConsumablesDataGridEventArgs args)
        {
            _materialId = args.MaterialId;
            _tableName = args.TableName;
            _isDataLoaded = false;
            await LoadData();
        }

        private string GetCellBackgroundColor(string operationType)
        {
            if(string.IsNullOrEmpty(operationType))
                return "Transparent";

            return operationType switch
            {
                WriteOffOperation => "#609C6B6B",
                IncomeOperation => "#607D9C6B"
            };
        }

        private string GetFormattedValue(string operationType, double value)
        {
            if (string.IsNullOrEmpty(operationType))
                return value.ToString();

            return operationType switch
            {
                WriteOffOperation => $"-{value}",
                IncomeOperation => $"+{value}",
                _ => value.ToString()
            };
        }

        private string GetOperationColor(string operationType)
        {
            if (string.IsNullOrEmpty(operationType))
                return "Black";

            return operationType switch
            {
                WriteOffOperation => "Red",
                IncomeOperation => "Green",
                _ => "Black"
            };
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            _materialId = navigationContext.Parameters["MaterialId"] as int? ?? 0;
            _tableName = navigationContext.Parameters["TableName"] as string ?? string.Empty;
            _isDataLoaded = false;
            _eventAggregator.GetEvent<UpdateOperationsConsumablesDataGridEvent>().Subscribe(UpdateOperationsConsumablesDataGrid);
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}