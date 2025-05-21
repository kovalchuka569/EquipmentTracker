using System.Collections.ObjectModel;
using System.Dynamic;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Text;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using Common.Logging;
using Core.Events.DataGrid;
using Core.Events.DataGrid.Consumables;
using Core.Services.Common.DataGridColumns;
using Core.Services.Consumables;
using Models.ConsumablesDataGrid;
using Prism.Events;
using Syncfusion.UI.Xaml.Grid;
using UI.ViewModels.TabControl;

namespace UI.ViewModels.Consumables
{
    public class ConsumablesDataGridSelectorViewModel : BindableBase, INavigationAware
    {
        private IRegionManager _regionManager;
        private readonly IAppLogger<ConsumablesDataGridSelectorViewModel> _logger;
        private readonly IConsumablesDataGridService _service;
        private EventAggregator _scopedEventAggregator;
        
        private string _tableName;
        
        private ObservableCollection<ConsumableItem> _items = new();
        private ConsumableItem _selectedItem;
        public ObservableCollection<ConsumableItem> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public ConsumableItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }
        
        public DelegateCommand<object> CellDoubleTappedCommand { get; }
        public DelegateCommand BackToFoldersCommand { get; }
        
        public ConsumablesDataGridSelectorViewModel(
            IAppLogger<ConsumablesDataGridSelectorViewModel> logger,
            IConsumablesDataGridService service)
        {
            _logger = logger;
            _service = service;

            CellDoubleTappedCommand = new DelegateCommand<object>(OnCellDoubleTapped);
            BackToFoldersCommand = new DelegateCommand(OnBackToFolders);
        }

        private void OnBackToFolders()
        {
            _regionManager.Regions["ConsumablesDataGridSelectorRegion"].RemoveAll();
        }
        
        private void OnCellDoubleTapped(object obj)
        {
            if (SelectedItem is ConsumableItem consumableItem)
            {
                _scopedEventAggregator.GetEvent<ConsumableSelectedEvent>().Publish(new ConsumableSelectedEventArgs
                {
                    ConsumableTableName = _tableName,
                    ConsumableItem = consumableItem
                });
            }
        }

        private async Task LoadDataAsync()
        {
            Items = await _service.GetDataAsync(_tableName);
        }
        
        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
            {
                _regionManager = scopedRegionManager;
            }
            if (navigationContext.Parameters["ScopedEventAggregator"] is EventAggregator scopedEventAggregator)
            {
                _scopedEventAggregator = scopedEventAggregator;
            }
            _tableName = navigationContext.Parameters["TableName"] as string;
            await LoadDataAsync();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext) { }
    }
}
