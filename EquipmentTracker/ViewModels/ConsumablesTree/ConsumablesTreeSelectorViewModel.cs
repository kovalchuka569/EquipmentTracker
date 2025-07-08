using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Prism.Mvvm;
using Prism.Commands;
using System.Windows.Forms;
using Common.Logging;
using Core.Events.TabControl;
using Core.Models.Consumables;
using Core.Services.Consumables;
using Microsoft.Extensions.Logging;
using Models.EquipmentTree;
using Prism.Events;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;
using IFileSystemItem = Core.Models.Consumables.IFileSystemItem;

namespace UI.ViewModels.ConsumablesTree
{
    public class ConsumablesTreeSelectorViewModel : BindableBase, INavigationAware
    {
        private SfTreeView _sfTreeView;
        private bool _openedFromMaterialSelector;
        
        private IConsumablesTreeService _consumablesTreeService;
        private IAppLogger<ConsumablesTreeSelectorViewModel> _logger;
        private EventAggregator _scopedEventAggregator;
        private IRegionManager _regionManager;
        
        private ObservableCollection<IFileSystemItem> _folders;
        private IFileSystemItem _selectedItem;
        
        private DelegateCommand<SfTreeView> _sfTreeViewLoadedCommand;
        private DelegateCommand<NodeExpandedCollapsedEventArgs> _nodeExpandedCommand;
        private DelegateCommand<NodeExpandedCollapsedEventArgs> _nodeCollapsedCommand;
        private DelegateCommand _contextMenuOpenedCommand;

        private bool _openContextMenuVisibility = true;
        private bool _initialRename = false;
        private bool _progressBarVisibility;

        public DelegateCommand<SfTreeView> SfTreeViewLoadedCommand =>
            _sfTreeViewLoadedCommand ??= new DelegateCommand<SfTreeView>(OnSfTreeViewLoaded);
        
        public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeExpandedCommand =>
            _nodeExpandedCommand ??= new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeCollapsedCommand =>
        _nodeCollapsedCommand ??= new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);

        public DelegateCommand ContextMenuOpenedCommand =>
            _contextMenuOpenedCommand ??= new DelegateCommand(OnContextMenuOpened);
        

        public ObservableCollection<IFileSystemItem> Folders
        {
            get => _folders;
            set => SetProperty(ref _folders, value);
        }

        public IFileSystemItem SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        public bool OpenContextMenuVisibility
        {
            get => _openContextMenuVisibility;
            set => SetProperty(ref _openContextMenuVisibility, value);
        }

        public bool ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => SetProperty(ref _progressBarVisibility, value);
        }
        
        public DelegateCommand OpenCommand { get; }

        public ConsumablesTreeSelectorViewModel(IConsumablesTreeService consumablesTreeService, IAppLogger<ConsumablesTreeSelectorViewModel> logger)
        {
            _consumablesTreeService = consumablesTreeService;
            _logger = logger;

            OpenCommand = new DelegateCommand(OnOpenFile);

            _folders = new ObservableCollection<IFileSystemItem>();
            
            LoadTreeAsync();
        }

        private void OnOpenFile()
        {
            if (SelectedItem is IFileSystemItem item)
            {
                var parameters = new NavigationParameters
                {
                    {"TableName", item.Name},
                    {"ScopedRegionManager", _regionManager},
                    {"ScopedEventAggregator", _scopedEventAggregator}
                };
                _regionManager.RequestNavigate("ConsumablesDataGridSelectorRegion", "ConsumablesDataGridSelectorView", parameters);
            }
        }

        // Hide OpenFile item context menu when selected node is folder and show when selected node is file
        private void OnContextMenuOpened()
        {
            OpenContextMenuVisibility = SelectedItem is File;
        }

        // Change folder icon when node expanded or collapsed
        private void OnNodeExpandedCollapsed(NodeExpandedCollapsedEventArgs args)
        {
            if (args.Node.Content is Folder folder)
            {
                if (args.Node.IsExpanded)
                {
                    folder.ImageIcon = "Assets/opened_folder.png";
                }
                else
                {
                    folder.ImageIcon = "Assets/folder.png";
                }
            }
            
        }
        
        
        // Load tree
        private async Task LoadTreeAsync()
        {
            ProgressBarVisibility = true;
            await Task.Delay(1000);
            
            var allFolders = await _consumablesTreeService.GetFoldersAsync();
            var allFiles = await _consumablesTreeService.GetFilesAsync();
            _folders = _consumablesTreeService.BuildHierachy(allFolders, allFiles);
            
            var tableNames = allFiles.Select(f => f.Name).ToList();
            var lowValueCounts = await _consumablesTreeService.GetLowValueCountsAsync(tableNames);

            foreach (var file in allFiles)
            {
                if (lowValueCounts.TryGetValue(file.Name, out int count))
                {
                  file.BadgeVisibility = count > 0; 
                  file.BadgeValue = count;
                }
                else
                {
                    file.BadgeVisibility = false;
                    file.BadgeValue = 0;
                }
            }
            Folders = new ObservableCollection<IFileSystemItem>(Folders);
            
            ProgressBarVisibility = false;
        }

        // Gets link to the SfTreeView
        private void OnSfTreeViewLoaded(SfTreeView sfTreeView)
        {
            _sfTreeView = sfTreeView;
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
