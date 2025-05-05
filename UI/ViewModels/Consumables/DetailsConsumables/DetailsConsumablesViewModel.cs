using Core.Events.DataGrid.Consumables;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class DetailsConsumablesViewModel : BindableBase, INavigationAware
    {
        private readonly IRegionManager _regionManager;
        private readonly IEventAggregator _eventAggregator;
        
        private bool _isLoading = false;
        private bool _tipShowed = true;
        private bool _operationsShowed = false;
        private string _uniqueRegionName;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public bool TipShowed
        {
            get => _tipShowed;
            set => SetProperty(ref _tipShowed, value);
        }

        public string UniqueRegionName
        {
            get => _uniqueRegionName;
            set => SetProperty(ref _uniqueRegionName, value);
        }

        private DelegateCommand _unloadedUserControlCommand;
        private DelegateCommand _loadedUserControlCommand;

        public DelegateCommand UnloadedUserControlCommand =>
            _unloadedUserControlCommand ??= new DelegateCommand(OnUnloadedUserControl);
        
        public DelegateCommand LoadedUserControlCommand =>
            _loadedUserControlCommand ??= new DelegateCommand(OnLoadedUserControl);
        
        public DetailsConsumablesViewModel(IRegionManager regionManager,
            IEventAggregator eventAggregator)
        {
            _regionManager = regionManager;
            _eventAggregator = eventAggregator;
        }

        private void OnLoadedUserControl()
        {
            Console.WriteLine("DetailsConsumablesViewModel Loaded");
            _eventAggregator.GetEvent<OnSelectionRecordChanged>().Subscribe(ShowOperationsDataGrid);
        }

        private void OnUnloadedUserControl ()
        {
            Console.WriteLine("DetailsConsumablesViewModel Unloaded");
            _eventAggregator.GetEvent<OnSelectionRecordChanged>().Unsubscribe(ShowOperationsDataGrid);
        }

        private async void ShowOperationsDataGrid(SelectionRecordChangedEventArgs args)
        {
            TipShowed = false;
            IsLoading = true;
            await Task.Delay(500);

            if (_operationsShowed == false)
            {
                _regionManager.RequestNavigate("OperationsConsumablesRegion", "OperationsDataGridView");
                _operationsShowed = true;
            }
            
            _eventAggregator.GetEvent<UpdateOperationsConsumablesDataGridEvent>().Publish(new UpdateOperationsConsumablesDataGridEventArgs
            {
                MaterialId = args.MaterialId,
                TableName = args.TableName
            });
            IsLoading = false;
        } 

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Console.WriteLine("OnNavigatedTo DetailsConsumablesViewModel");
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

