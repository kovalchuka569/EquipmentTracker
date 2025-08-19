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
using Prism.Mvvm;
using Prism.Commands;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls;
using Timer = System.Timers.Timer;
using Core.Events.TabControl;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class OperationsDataGridViewModel : BindableBase, INavigationAware
    {
        private readonly IOperationsDataGridService _service;
        private readonly IEventAggregator _eventAggregator;
        private EventAggregator _scopedEventAggregator;
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
        private string _operationsTableName;
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
            ImageViewerTempStorage imageViewerTempStorage)
        {
            _service = service;
            _eventAggregator = eventAggregator;
            _imageViewerTempStorage = imageViewerTempStorage;
            
            DataGridLoadedCommand = new DelegateCommand<SfDataGrid>(OnDataGridLoaded);
            ShowAddNewTemplateCommand = new DelegateCommand(OnShowAddNewTemplate);
            ViewReceiptCommand = new DelegateCommand<object>(OnViewReceipt);
        }

        private void OnViewReceipt(object parameter)
        {
            if (parameter is Operation operation)
            {
                _imageViewerTempStorage.ImageData = operation.Receipt;
                _imageViewerTempStorage.ImageName = "Квитанція на " + operation.OperationType + " від " + operation.DateTime.ToString("yyyy-MM-dd_HH-mm-ss");
                _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs 
                { 
                    Header = "Квитанція на " + operation.OperationType + " від " + operation.DateTime,
                    Parameters = new Dictionary<string, object>
                    {
                        { "ViewNameToShow", "ImageViewerView" }
                    }
                });
            }
        }

        private void OnShowAddNewTemplate()
        {
            _isAddingNewOperation = true;
            IsOverlayVisible = true;
            var parameters = new NavigationParameters();
            parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
            _regionManager.RequestNavigate("AddNewOperationTemplateRegion", "AddNewOperationView", parameters);
            _scopedEventAggregator.GetEvent<CloseAddNewTemplateEvent>().Subscribe(OnCloseAddNewTemplate);
            _scopedEventAggregator.GetEvent<AddNewOperationEvent>().Subscribe(OnAddNewOperation);
        }

        private void OnCloseAddNewTemplate()
        {
            _regionManager.Regions["AddNewOperationTemplateRegion"].RemoveAll();
            _scopedEventAggregator.GetEvent<CloseAddNewTemplateEvent>().Unsubscribe(OnCloseAddNewTemplate);
            _isAddingNewOperation = false;
            IsOverlayVisible = false;
        }

        private async void OnAddNewOperation(AddNewOperationEventArgs args)
        {

           await _service.InsertRecordAsync(_operationsTableName, _tableName, _materialId, args.OperationType, args.DateTime.ToString(), args.Quantity,args.Description, args.User, args.ReceiptImageBytes);
            _scopedEventAggregator.GetEvent<AddNewOperationEvent>().Unsubscribe(OnAddNewOperation);
            _isDataLoaded = false;
            await LoadData();
        }

        private async void OnDataGridLoaded(SfDataGrid dataGrid)
        {
            _operationsDataGrid = dataGrid;
        }

        private async Task LoadData()
        {
            Console.WriteLine("LoadData1");
            if (_isDataLoaded || _isAddingNewOperation)
            {
                return;
            }

            try
            {
                Console.WriteLine("LoadData");
                Operations.Clear();
                var data = await _service.GetDataAsync(_operationsTableName, _materialId);
                
                if (data.Count == 0) NullOperationsTipVisibility = true;
                if(data.Count > 0) NullOperationsTipVisibility = false;

                var operations = data.Select(dto => new Operation
                {
                    Id = dto.Id,
                    OperationType = dto.OperationType,
                    Quantity = dto.Quantity,
                    BalanceAfter = dto.BalanceAfter,
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
            }
        }

        private async void UpdateOperationsConsumablesDataGrid(UpdateOperationsConsumablesDataGridEventArgs args)
        {
            _materialId = args.MaterialId;
            _operationsTableName = args.OperationsTableName;
            _tableName = args.TableName;
            _isDataLoaded = false;
            await LoadData();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
            {
                _regionManager = scopedRegionManager;
            }
            if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
            {
                _scopedEventAggregator = scopedEventAggregator;
            }
            _materialId = navigationContext.Parameters["MaterialId"] as int? ?? 0;
            _operationsTableName = navigationContext.Parameters["TableName"] as string ?? string.Empty;
            _isDataLoaded = false;
            _scopedEventAggregator.GetEvent<UpdateOperationsConsumablesDataGridEvent>().Subscribe(UpdateOperationsConsumablesDataGrid);
        }
        public bool IsNavigationTarget(NavigationContext navigationContext) => true;
        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}