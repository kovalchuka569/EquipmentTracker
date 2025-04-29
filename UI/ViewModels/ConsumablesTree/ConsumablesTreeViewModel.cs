using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Forms;
using Common.Logging;
using Core.Events.TabControl;
using Core.Models.Consumables;
using Core.Services.Consumables;
using Microsoft.Extensions.Logging;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;

namespace UI.ViewModels.ConsumablesTree
{
    public class ConsumablesTreeViewModel : BindableBase, INavigationAware
    {
        private SfTreeView _sfTreeView;
        private string _oldFileName;
        
        private IConsumablesTreeService _consumablesTreeService;
        private IAppLogger<ConsumablesTreeViewModel> _logger;
        private IEventAggregator _eventAggregator;
        
        private ObservableCollection<IFileSystemItem> _folders;
        private IFileSystemItem _selectedItem;
        
        private DelegateCommand<SfTreeView> _sfTreeViewLoadedCommand;
        private DelegateCommand _addFolderCommand;
        private DelegateCommand _addFileCommand;
        private DelegateCommand _openCommand;
        private DelegateCommand _editCommand;
        private DelegateCommand<TreeViewItemEndEditEventArgs> _itemEndEditCommand;
        private DelegateCommand<TreeViewItemBeginEditEventArgs> _itemBeginEditCommand;
        private DelegateCommand<NodeExpandedCollapsedEventArgs> _nodeExpandedCommand;
        private DelegateCommand<NodeExpandedCollapsedEventArgs> _nodeCollapsedCommand;
        private DelegateCommand _contextMenuOpenedCommand;

        private bool _openContextMenuVisibility = true;
        private bool _initialRename = false;

        public DelegateCommand<SfTreeView> SfTreeViewLoadedCommand =>
            _sfTreeViewLoadedCommand ??= new DelegateCommand<SfTreeView>(OnSfTreeViewLoaded);
        
        public DelegateCommand AddFolderCommand =>
            _addFolderCommand ??= new DelegateCommand(OnAddFolder);
        public DelegateCommand AddFileCommand =>
            _addFileCommand ??= new DelegateCommand(OnAddFile);
        public DelegateCommand OpenCommand =>
            _openCommand ??= new DelegateCommand(OnOpenNode);
        public DelegateCommand EditCommand =>
            _editCommand ??= new DelegateCommand(OnEdit);
        
        public DelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand =>
            _itemEndEditCommand ??= new DelegateCommand<TreeViewItemEndEditEventArgs>(OnItemEndEdit);
        public DelegateCommand<TreeViewItemBeginEditEventArgs> ItemBeginEditCommand =>
            _itemBeginEditCommand ??= new DelegateCommand<TreeViewItemBeginEditEventArgs>(OnItemBeginEdit);
        
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

        public ConsumablesTreeViewModel(IConsumablesTreeService consumablesTreeService, IAppLogger<ConsumablesTreeViewModel> logger, IEventAggregator eventAggregator, ILoggerFactory loggerFactory)
        {
            _consumablesTreeService = consumablesTreeService;
            _logger = logger;
            _eventAggregator = eventAggregator;

            LoadTreeAsync();
        }

        private void OnEdit()
        {
            SendKeys.SendWait("{F2}");
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

        private async void OnAddFolder()
        {
            try
            {
                int? parentId = SelectedItem is Folder folder ? folder.Id : null;
                string baseName = "Нова категорія";
                string uniqueName = await _consumablesTreeService.GenerateUniqueFolderNameAsync(baseName, parentId); // Get unique folder name
                var newFolder = new Folder
                {
                    Name = uniqueName,
                    ParentId = parentId,
                };
                await _consumablesTreeService.InsertFolderAsync(newFolder);

                if (SelectedItem is Folder parentFolder)
                {
                    parentFolder.AddFolder(newFolder);
                    var parentNode = FindTreeNode(_sfTreeView, parentFolder);
                    
                    if (parentNode != null && !parentNode.IsExpanded)
                    {
                        _sfTreeView.ExpandNode(parentNode);
                        if (parentNode.Content is Folder pFolder)
                        {
                            pFolder.ImageIcon = "Assets/opened_folder.png";
                        }
                    }
                }
                else
                {
                    Folders.Add(newFolder);
                }
                
                SelectedItem = newFolder;
                SendKeys.SendWait("{F2}");
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private async void OnAddFile()
        {
            if (SelectedItem is not Folder parentFolder)
                return;

            try
            {
                string baseName = "Нова таблиця";
                string uniqueName = await _consumablesTreeService.GenerateUniqueFileNameAsync(baseName, parentFolder.Id);

                var newFile = new File
                {
                    Name = uniqueName,
                    ParentIdFolder = parentFolder.Id,
                };

                await _consumablesTreeService.InsertFileAsync(newFile);
                parentFolder.AddFile(newFile);

                var parentNode = FindTreeNode(_sfTreeView, parentFolder);
                if (parentNode is { IsExpanded: false } && parentNode.Content is Folder pFolder)
                {
                    _sfTreeView.ExpandNode(parentNode);
                    pFolder.ImageIcon = "Assets/opened_folder.png";
                }

                SelectedItem = newFile;
                SendKeys.SendWait("{F2}");
                _initialRename = true;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        private void OnItemBeginEdit(TreeViewItemBeginEditEventArgs args)
        {
            if (args.Node.Content is File file)
            {
                _oldFileName = file.Name;
            }
        }

        private async void OnItemEndEdit(TreeViewItemEndEditEventArgs args)
        {
            try
            {
                if (args.Node.Content is File file)
                {
                    string userInputName = file.Name;
                    string uniqueName = await _consumablesTreeService.GenerateUniqueFileNameAsync(userInputName, file.ParentIdFolder);
                    if (uniqueName != file.Name)
                    {
                        file.Name = uniqueName;
                        await _consumablesTreeService.RenameFileAsync(uniqueName, _oldFileName, file.Id);
                    }
                    else
                    {
                        await _consumablesTreeService.RenameFileAsync(file.Name, _oldFileName, file.Id);
                    }

                    if (_initialRename)
                    {
                        _eventAggregator.GetEvent<OnOpenConsumablesFileEvent>().Publish(file.Name);
                        _initialRename = false;
                    }
                }

                if (args.Node.Content is Folder folder)
                {
                    string userInputName = folder.Name;
                    string uniqueName = await _consumablesTreeService.GenerateUniqueFolderNameAsync(userInputName, folder.ParentId);
                    if (uniqueName != folder.Name)
                    {
                        folder.Name = uniqueName;
                        await _consumablesTreeService.RenameFolderAsync(uniqueName, folder.Id);
                    }
                    else
                    {
                        await _consumablesTreeService.RenameFolderAsync(folder.Name, folder.Id);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message, "Error on rename node");
                throw;
            }
        }

        private void OnOpenNode()
        {
            if (SelectedItem is File file)
            {
                _eventAggregator.GetEvent<OnOpenConsumablesFileEvent>().Publish(file.Name);
            }
        }
        
        // Load tree
        private async void LoadTreeAsync()
        {
            var allFolders = await _consumablesTreeService.GetFoldersAsync();
            var allFiles = await _consumablesTreeService.GetFilesAsync();
            Folders = _consumablesTreeService.BuildHierachy(allFolders, allFiles);
        }

        // Gets link to the SfTreeView
        private void OnSfTreeViewLoaded(SfTreeView sfTreeView)
        {
            _sfTreeView = sfTreeView;
        }
        
        private TreeViewNode FindTreeNode(SfTreeView treeView, Folder folder)
        {
            foreach (var node in treeView.Nodes)
            {
                if (node.Content == folder)
                    return node;
                
                var foundInChild = FindNodeRecursive(node, folder);
                if (foundInChild != null)
                    return foundInChild;
            }
            return null;
        }

        private TreeViewNode FindNodeRecursive(TreeViewNode parentNode, Folder folder)
        {
            foreach (var childNode in parentNode.ChildNodes)
            {
                if (childNode.Content == folder)
                    return childNode;
        
                var foundInNested = FindNodeRecursive(childNode, folder);
                if (foundInNested != null)
                    return foundInNested;
            }
            return null;
        }
        
        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext) => true;

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
