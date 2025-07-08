using Core.Events.DataGrid.Consumables;

namespace UI.ViewModels.Consumables.DetailsConsumables
{
    public class DetailsConsumablesViewModel : BindableBase, INavigationAware
    {
        private IRegionManager _regionManager;
        private EventAggregator _scopedEventAggregator;
        
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
        

        private void OnLoadedUserControl()
        {
            _scopedEventAggregator.GetEvent<OnSelectionRecordChanged>().Subscribe(ShowOperationsDataGrid);
        }

        private void OnUnloadedUserControl ()
        {
            _scopedEventAggregator.GetEvent<OnSelectionRecordChanged>().Unsubscribe(ShowOperationsDataGrid);
        }

        private async void ShowOperationsDataGrid(SelectionRecordChangedEventArgs args)
        {
            TipShowed = false;
            IsLoading = true;
            await Task.Delay(500);

            if (_operationsShowed == false)
            {
                var parameters = new NavigationParameters();
                parameters.Add("ScopedRegionManager", _regionManager);
                parameters.Add("ScopedEventAggregator", _scopedEventAggregator);
                _regionManager.RequestNavigate("OperationsConsumablesRegion", "OperationsDataGridView", parameters);
                _operationsShowed = true;
            }
            
            _scopedEventAggregator.GetEvent<UpdateOperationsConsumablesDataGridEvent>().Publish(new UpdateOperationsConsumablesDataGridEventArgs
            {
                MaterialId = args.MaterialId,
                OperationsTableName = args.OperationsTableName,
                TableName = args.TableName,
            });
            IsLoading = false;
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
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}

