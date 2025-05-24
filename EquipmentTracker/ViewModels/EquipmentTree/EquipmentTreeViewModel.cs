using System.Collections.ObjectModel;
using System.Windows;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Commands;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Core.Events;
using Core.Events.EquipmentTree;
using Core.Events.TabControl;
using Core.Models.EquipmentTree;
using Core.Models.Tabs.EquipmentTree;
using Core.Services.EquipmentTree;
using Syncfusion.UI.Xaml.TreeView;


using Notification.Wpf;

using Data.Entities;
using MaterialDesignThemes.Wpf;
using Models.EquipmentTree;
using Notification.Wpf.Classes;
using Prism.Events;
using Syncfusion.UI.Xaml.TreeView.Engine;
using Syncfusion.UI.Xaml.TreeView.Helpers;
using UI.ViewModels.TabControl;
using DelegateCommand = Prism.Commands.DelegateCommand;
using Application = System.Windows.Application;

namespace UI.ViewModels.EquipmentTree;

public class EquipmentTreeViewModel : BindableBase, INavigationAware, IRegionCleanup
{
    #region Properties
    private readonly IEquipmentTreeService _service;
    private readonly IEventAggregator _eventAggregator;
    private IEventAggregator _scopedEventAggregator;
    private IRegionManager _regionManager;
    private bool _isFirstRegionInitialized;
    private bool _isSecondRegionInitialized;

    
    private SfTreeView _sfTreeView;
    
    private bool _columnSelectorVisibility = false;


    private string _menuType;
    #endregion
    
    

    #region Services

    private ObservableCollection<IFileSystemItem> _items = new();
    private IFileSystemItem _selectedItem;
    private string _columnSelectorRegionName;
    private TaskCompletionSource<bool> _tableCreationTcs;
    private SnackbarMessageQueue _messageQueue = new();
    
    //Context menu items visibility
    private bool _openContextMenuVisibility;
    private bool _addFolderFileContextMenuItemVisibility;
    private bool _editContextMenuItemVisibility;

    private bool _emptyDataTipVisibility;
    private bool _isColumnSelectorVisible;
    private bool _progressBarVisibility;
    private bool _isOverlayVisible;
    
    public bool ColumnSelectorVisibility
    {
        get => _columnSelectorVisibility;
        set => SetProperty(ref _columnSelectorVisibility, value);
    }

    public ObservableCollection<IFileSystemItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public IFileSystemItem SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }
    
    public string ColumnSelectorRegionName
    {
        get => _columnSelectorRegionName;
        set => SetProperty(ref _columnSelectorRegionName, value);
    }

    public bool OpenContextMenuVisibility
    {
        get => _openContextMenuVisibility;
        set => SetProperty(ref _openContextMenuVisibility, value);
    }

    public bool AddFolderFileContextMenuItemVisibility
    {
        get => _addFolderFileContextMenuItemVisibility;
        set => SetProperty(ref _addFolderFileContextMenuItemVisibility, value);
    }

    public bool EditContextMenuItemVisibility
    {
        get => _editContextMenuItemVisibility;
        set => SetProperty(ref _editContextMenuItemVisibility, value);
    }

    public bool EmptyDataTipVisibility
    {
        get => _emptyDataTipVisibility;
        set => SetProperty(ref _emptyDataTipVisibility, value);
    }

    public SnackbarMessageQueue MessageQueue
    {
        get => _messageQueue;
        set => SetProperty(ref _messageQueue, value);
    }

    public bool IsColumnSelectorVisible
    {
        get => _isColumnSelectorVisible;
        set => SetProperty(ref _isColumnSelectorVisible, value);
    }

    public bool ProgressBarVisibility
    {
        get => _progressBarVisibility;
        set => SetProperty(ref _progressBarVisibility, value);
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetProperty(ref _isOverlayVisible, value);
    }
    

    #endregion
    
    public DelegateCommand<SfTreeView> SfTreeViewLoadedCommand { get; }
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeExpandedCommand { get; }
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeCollapsedCommand { get; }
    public DelegateCommand AddFolderCommand { get; }
    public DelegateCommand AddFileCommand { get; }
    public DelegateCommand EditCommand { get; }
    public DelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand { get; }
    public DelegateCommand ContextMenuOpenedCommand { get; }
    public DelegateCommand OpenCommand { get; }
    
    #region Constructor
    public EquipmentTreeViewModel(IEquipmentTreeService service, IEventAggregator eventAggregator)
    {
        _scopedEventAggregator = new EventAggregator();
        _eventAggregator = eventAggregator;
        _service = service;

        SfTreeViewLoadedCommand = new DelegateCommand<SfTreeView>(OnSfTreeViewLoaded);
        NodeExpandedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        NodeCollapsedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        AddFolderCommand = new DelegateCommand(OnAddFolder);
        AddFileCommand = new DelegateCommand(OnAddFile);
        EditCommand = new DelegateCommand(OnEdit);
        ItemEndEditCommand = new DelegateCommand<TreeViewItemEndEditEventArgs>(OnItemEndEdit);
        ContextMenuOpenedCommand = new DelegateCommand(OnContextMenuOpened);
        OpenCommand = new DelegateCommand(OnOpenFileCommand);
    }
    #endregion

    private void OnOpenFileCommand()
    {
        if (SelectedItem is FileItem fileItem)
        {
            string viewName = string.Empty;
            switch (fileItem.FileType)
            {
                case "equipments table":
                    viewName = "EquipmentDataGridView";
                    break;

                case "writeoff":
                    viewName = "WriteoffDataGridView";
                    break;

                case "services":
                    viewName = "ServicesDataGridView";
                    break;

                case "repairs":
                    viewName = "RepairsDataGridView";
                    break;
            }
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = fileItem.Name,
                Parameters = new Dictionary<string, object>
                {
                    { "ViewNameToShow", viewName },
                    { "EquipmentDataGridView.TableName", fileItem.TableName },
                    { "RepairsDataGridView.RepairsTableName", fileItem.TableName },
                    { "ServicesDataGridView.ServicesTableName", fileItem.TableName }
                }
            });
        }
    }
    
    private string _oldFolderName;
    private void OnEdit()
    {
        var node = FindNodeByContent(_sfTreeView.Nodes, SelectedItem);
        if (node != null)
        {
            _sfTreeView.BeginEdit(node);
            if (SelectedItem is FolderItem folder)
            {
                _oldFolderName = folder.Name;
            }
        }
    }
    
    private IEnumerable<FolderItem> GetAllFolderItems(IEnumerable<IFileSystemItem> items)
    {
        foreach (var folder in items.OfType<FolderItem>())
        {
            yield return folder;

            foreach (var subFolder in GetAllFolderItems(folder.SubItems))
            {
                yield return subFolder;
            }
        }
    }

    private bool ValidateName(string newName, string oldName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            MessageQueue.Enqueue("Назва не може бути порожньою!");
            return false;
        }
        if(newName == oldName) return true;
        if (newName.Length > 55)
        {
            MessageQueue.Enqueue("Максимальна довжина 55 символів!");
            return false;
        }
        string[] forbiddenSubstrings = { "--", ";", "'", "\"", "/*", "*/", "@@", "char", "nchar", "varchar", "nvarchar", "alter", "begin", "cast", "create", "cursor", "declare", "delete", "drop", "end", "exec", "execute", "fetch", "insert", "kill", "open", "select", "sys", "sysobjects", "syscolumns", "table", "update" };

        foreach (var item in forbiddenSubstrings)
        {
            if (newName.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                MessageQueue.Enqueue("Назва містить заборонені символи або слова!");
                return false;
            }
        }
        return true;
    }
    
    private async void OnItemEndEdit(TreeViewItemEndEditEventArgs args)
    {
        if (args.Node.Content is FolderItem folder)
        {
            string oldName = _oldFolderName;
            string newName = folder.Name;

            if (!ValidateName(newName, oldName))
            {
                folder.Name = _oldFolderName;
                return;
            }

            var folderNames = GetAllFolderItems(Items)
                .Where(f => f.Id != folder.Id)
                .Select(f => f.Name)
                .ToList();

            string uniqueName = await _service.GenerateUniqueFileFolderNameAsync(newName, folderNames);

            bool hasFiles = folder.SubItems.OfType<FileItem>().Any();

            if (uniqueName != folder.Name)
            {
                if (hasFiles)
                {
                    RenameAllSubFileItems(folder, oldName, uniqueName);
                    await _service.RenameChildsAsync(folder.Id, uniqueName, oldName, _menuType);
                }

                folder.Name = uniqueName;
                await _service.RenameFolderAsync(folder.Id, uniqueName);
            }
            else if (hasFiles)
            {
                RenameAllSubFileItems(folder, oldName, folder.Name);
                await _service.RenameChildsAsync(folder.Id, folder.Name, oldName, _menuType);
                await _service.RenameFolderAsync(folder.Id, folder.Name);
            }
            else
            {
                await _service.RenameFolderAsync(folder.Id, folder.Name);
            }
        }
    }
    
    private void RenameAllSubFileItems(IFileSystemItem item, string oldName, string newName)
    {
        if (item is FileItem file && file.Name.Contains(oldName))
        {
            file.Name = file.Name.Replace(oldName, newName);
        }
        else if (item is FolderItem folder)
        {
            if (folder.Name.Contains(oldName))
            {
                folder.Name = folder.Name.Replace(oldName, newName);
            }

            foreach (var subItem in folder.SubItems)
            {
                RenameAllSubFileItems(subItem, oldName, newName);
            }
        }
    }

    private TreeViewNode FindNodeByContent(TreeViewNodeCollection nodes, object content)
    {
        foreach (TreeViewNode node in nodes)
        {
            if(node.Content == content) return node;
            
            var foundInChildren = FindNodeByContent(node.ChildNodes, content);
            if (foundInChildren != null) return foundInChildren;
        }
        return null;
    }

    private async void OnSfTreeViewLoaded(SfTreeView sfTreeView)
    {
        _sfTreeView = sfTreeView;
    }

    private async void LoadTreeAsync()
    {
        ProgressBarVisibility = true;
        await Task.Delay(500);
        _items = await _service.BuildHierachy(_menuType);
        Items = new ObservableCollection<IFileSystemItem>(_items);
        CheckEmptyData();
        ProgressBarVisibility = false;
    }

    private void CheckEmptyData()
    {
        if(Items.Count == 0) EmptyDataTipVisibility = true;
        else EmptyDataTipVisibility = false;
    }

    private void OnNodeExpandedCollapsed(NodeExpandedCollapsedEventArgs args)
    {
        if (args.Node.Content is FolderItem folder)
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

    private IEnumerable<string> GetAllFolderNames(IEnumerable<IFileSystemItem> items)
    {
        return items
            .OfType<FolderItem>()
            .SelectMany(folder =>
                new[] { folder.Name }.Concat(GetAllFolderNames(folder.SubItems)));
    }

    private async void OnAddFolder()
    {
        int? parentId = SelectedItem is FolderItem folderItem ? folderItem.Id : 0;
        string baseName = "Нова папка";
        var folderNames = GetAllFolderNames(Items).ToList();
        string uniqueName = await _service.GenerateUniqueFileFolderNameAsync(baseName, folderNames);
        int newId = await _service.InsertFolderAsync(uniqueName, parentId, _menuType);
        var newFolder = new FolderItem
        {
            Name = uniqueName,
            Id = newId
        };

        if (SelectedItem is FolderItem parentFolder)
        {
            parentFolder.AddFolder(newFolder);
            var parentNode = FindTreeNode(_sfTreeView, parentFolder);

            if (parentNode != null && !parentNode.IsExpanded)
            {
                _sfTreeView.ExpandNode(parentNode);
                if (parentNode.Content is FolderItem folder)
                {
                    folder.ImageIcon = "Assets/opened_folder.png";
                }
            }
        }
        else
        {
            Items.Add(newFolder);
        }
        
        SelectedItem = newFolder;
        OnEdit();
        CheckEmptyData();
    }

    private async void OnAddFile()
    {
        int folderId = SelectedItem is FolderItem folderItem ? folderItem.Id : 0;
        string fileName = SelectedItem.Name;
        string menuType = _menuType;

        IsOverlayVisible = true;
        bool creationStatus = await ShowColumnSelectorAsync(fileName);
        IsOverlayVisible = false;

        if (creationStatus)
        {
            int newId = await _service.InsertFileAsync(fileName, folderId, menuType);
            var newFile = new FileItem
            {
                Name = fileName,
                FileType = "equipments table",
                TableName = $"{fileName}",
                Id = newId
            };
            var newFolder = new FolderItem
            {
                Name = $"{fileName} технічні роботи",
            };
            var newServiceFile = new FileItem
            {
                Name = $"{fileName} обслуговування",
                TableName = $"{fileName} О",
                FileType = "services",
            };
            var newRepairsFile = new FileItem
            {
                Name = $"{fileName} ремонти",
                TableName = $"{fileName} Р",
                FileType = "repairs",
            };
            var newWriteOffFile = new FileItem
            {
                Name = $"{fileName} списані",
                TableName = $"{fileName}",
                FileType = "writeoff",
            };
            if (SelectedItem is FolderItem folder)
            {
                folder.AddFile(newFile);
                folder.AddFile(newWriteOffFile);
                folder.AddFolder(newFolder);
                newFolder.AddFile(newServiceFile);
                newFolder.AddFile(newRepairsFile);
            }
        }
    }
    
    private TreeViewNode FindTreeNode(SfTreeView treeView, FolderItem folder)
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
    
    private TreeViewNode FindNodeRecursive(TreeViewNode parentNode, FolderItem folder)
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

    private void OnContextMenuOpened()
    {
        EditContextMenuItemVisibility = false;
        AddFolderFileContextMenuItemVisibility = true;
        if (SelectedItem is FileItem)
        {
            OpenContextMenuVisibility = true;
            EditContextMenuItemVisibility = false;
            AddFolderFileContextMenuItemVisibility = false;
        }
        if (SelectedItem is FolderItem folderItem)
        {
            EditContextMenuItemVisibility = true;
            AddFolderFileContextMenuItemVisibility = true;  
            if (folderItem.Name.Contains("обслуговування", StringComparison.OrdinalIgnoreCase))
            {
                EditContextMenuItemVisibility = false;
                AddFolderFileContextMenuItemVisibility = false;
            }
            if (folderItem.SubItems.Any(child => child is FileItem))
            {
                AddFolderFileContextMenuItemVisibility = false;
            }
        }
    }
    
   private async Task<bool> ShowColumnSelectorAsync(string fileName)
   {
       _tableCreationTcs = new TaskCompletionSource<bool>();
    
       var parameters = new NavigationParameters
       {
           {"TableName", fileName},
           {"ScopedRegionManager", _regionManager},
           {"ScopedEventAggregator", _scopedEventAggregator}
       };
       
       SubscriptionToken subscriptionToken = null;
       subscriptionToken = _scopedEventAggregator.GetEvent<TableCreatingSuccessfullyEvent>()
           .Subscribe(isSuccess =>
               {
                   _tableCreationTcs.TrySetResult(isSuccess);
                   if (subscriptionToken != null)
                   {
                       _scopedEventAggregator.GetEvent<TableCreatingSuccessfullyEvent>()
                           .Unsubscribe(subscriptionToken);
                   }
               }, 
               ThreadOption.UIThread, 
               keepSubscriberReferenceAlive: true);
       
       _regionManager.RequestNavigate("ColumnSelectorRegion", "ColumnSelectorView", parameters);
       
       var result = await _tableCreationTcs.Task;
       return result;
   }
   
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        var parameters = navigationContext.Parameters["MenuType"] as string;
        ColumnSelectorRegionName = navigationContext.Parameters.GetValue<string>("ColumnSelectorRegionName");
        _menuType = parameters;
        LoadTreeAsync();
    }
    

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }

    public void CleanupRegions()
    {
        _regionManager.Regions.Remove(ColumnSelectorRegionName);
    }
}