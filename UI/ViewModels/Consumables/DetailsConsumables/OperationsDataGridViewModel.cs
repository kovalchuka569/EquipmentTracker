using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Threading.Tasks;
using Core.Events.Common;
using Core.Events.DataGrid.Consumables;
using Core.Services.Common;
using Core.Services.Consumables.Operations;
using Core.Services.DataGrid;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls;
using UI.ViewModels.TabControl;
using Timer = System.Timers.Timer;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class OperationsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IOperationsDataGridService _service;
        private readonly IEventAggregator _eventAggregator;
        private IRegionManager _regionManager;
        private ImageViewerTempStorage _imageViewerTempStorage;

        private ObservableCollection<Operation> _operations = new();
        private Operation _selectedOperation;

        public ObservableCollection<Operation> Operations
        {
            get => _operations;
            set => SetProperty(ref _operations, value);
        }

        public Operation SelectedOperation
        {
            get => _selectedOperation;
            set => SetProperty(ref _selectedOperation, value);
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

        public DelegateCommand<SfDataGrid> DataGridLoadedCommand { get; }
        public DelegateCommand ShowAddNewTemplateCommand { get; }
        public DelegateCommand<object> ViewReceiptCommand  { get; }

        public OperationsDataGridViewModel(
            IOperationsDataGridService service,
            IEventAggregator eventAggregator, 
            IRegionManager regionManager,
            ImageViewerTempStorage imageViewerTempStorage)
        {
            _service = service;
            _eventAggregator = eventAggregator;
            _regionManager = regionManager;
            _imageViewerTempStorage = imageViewerTempStorage;
            
            DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
            ShowAddNewTemplateCommand = new DelegateCommand(OnShowAddNewTemplate);
            ViewReceiptCommand = new DelegateCommand<object>(OnViewReceipt);
        }

        private void OnViewReceipt(object parameter)
        {
            if (parameter is Operation operation)
            {
                Console.WriteLine(operation.Receipt.Length+" ImageDataFrom operations");
                _imageViewerTempStorage.ImageData = operation.Receipt;
                _imageViewerTempStorage.ImageName = "Квитанція на " + operation.OperationType + " від " + operation.DateTime;
                _eventAggregator.GetEvent<OpenImageViewerEvent>().Publish("Квитанція на " + operation.OperationType + " від " + operation.DateTime);
            }
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
           await _service.InsertRecordAsync(_tableName, _materialId, args.OperationType, args.DateTime.ToString(), args.Quantity, args.Description, args.User, args.ReceiptImageBytes);
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
            var stopwatch = Stopwatch.StartNew();
            if (_isDataLoaded || _isAddingNewOperation)
            {
                return;
            }

            try
            {
                Operations.Clear();
                var data = await _service.GetDataAsync(_tableName, _materialId);
                
                if (data.Count == 0) NullOperationsTipVisibility = true;
                if(data.Count > 0) NullOperationsTipVisibility = false;

                var operations = data.Select(dto => new Operation
                {
                    Id = dto.Id,
                    OperationType = dto.OperationType,
                    Quantity = dto.Quantity,
                    DateTime = dto.DateTime,
                    Description = dto.Description,
                    Worker = dto.Worker,
                    Receipt = dto.Receipt,
                }).ToList();
                
                foreach (var operation in operations)
                {
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
                stopwatch.Stop();
                Console.WriteLine("загрузка заняла: " +  stopwatch.ElapsedMilliseconds);
            }
        }

        private async void UpdateOperationsConsumablesDataGrid(UpdateOperationsConsumablesDataGridEventArgs args)
        {
            _materialId = args.MaterialId;
            _tableName = args.TableName;
            _isDataLoaded = false;
            await LoadData();
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