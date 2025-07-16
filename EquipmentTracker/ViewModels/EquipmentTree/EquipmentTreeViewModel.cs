using System.Collections.ObjectModel;
using System.ComponentModel;
using Core.Events.TabControl;
using Core.Services.EquipmentTree;
using Syncfusion.UI.Xaml.TreeView;
using MaterialDesignThemes.Wpf;
using Models.EquipmentTree;
using Models.NavDrawer;
using Syncfusion.UI.Xaml.TreeView.Engine;
using UI.ViewModels.TabControl;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace UI.ViewModels.EquipmentTree;

public class EquipmentTreeViewModel : BindableBase, INavigationAware, IRegionMemberLifetime, IDestructible
{
    #region Properties
    private readonly IEquipmentTreeService _service;
    private readonly IEventAggregator _eventAggregator;
    private IEventAggregator _scopedEventAggregator;
    private IRegionManager _regionManager;

    
    private SfTreeView _sfTreeView;
    
    private bool _columnSelectorVisibility = false;


    private MenuType _menuType;
    #endregion

    #region Services

    private ObservableCollection<IFileSystemItem> _items = new();
    private IFileSystemItem _selectedItem;
    private SnackbarMessageQueue _messageQueue = new();
    
    //Context menu items visibility
    public bool OpenContextMenuVisibility => SelectedItem is FileItem;
    public bool ConnectionsContextMenuVisibility => SelectedItem is FileItem;
    public bool ConnectEquipmentContextMenuVisibility => SelectedItem is FileItem {FileFormat: FileFormat.RepairsSheet or FileFormat.ServicesSheet or FileFormat.WriteOffSheet};
    public bool CreateContextMenuItemVisibility => SelectedItem is not FileItem;
    public bool EditContextMenuItemVisibility => SelectedItem is FileItem or FolderItem;
    
    private bool _emptyDataTipVisibility;
    private bool _progressBarVisibility;
    private bool _isOverlayVisible;
    private string _searchText;

    public ObservableCollection<IFileSystemItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public IFileSystemItem SelectedItem
    {
        get => _selectedItem;
        set { SetProperty(ref _selectedItem, value); RaiseContextMenuItemsVisibilityChanged();}
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

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    #endregion
    
    public DelegateCommand<SfTreeView> SfTreeViewLoadedCommand { get; }
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeExpandedCommand { get; }
    public DelegateCommand<NodeExpandedCollapsedEventArgs> NodeCollapsedCommand { get; }
    public DelegateCommand AddFolderCommand { get; }
    public DelegateCommand EditCommand { get; }
    public DelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand { get; }
    public DelegateCommand ContextMenuOpenedCommand { get; }
    public DelegateCommand OpenCommand { get; }
    public DelegateCommand AddEquipmentsFileCommand { get; }
    public DelegateCommand AddRepairsFileCommand { get; }
    public DelegateCommand AddServicesFileCommand { get; }
    public DelegateCommand AddWriteOffFileCommand { get; }
    public DelegateCommand AddEquipmentsSummaryReportCommand { get; }
    public DelegateCommand AddRepairsSummaryReportCommand { get; }
    public DelegateCommand AddServicesSummaryReportCommand { get; }
    public DelegateCommand AddWriteOffSummaryReportCommand { get; }
    public EquipmentTreeViewModel(IEquipmentTreeService service, IEventAggregator eventAggregator)
    {
        _scopedEventAggregator = new EventAggregator();
        _eventAggregator = eventAggregator;
        _service = service;

        SfTreeViewLoadedCommand = new DelegateCommand<SfTreeView>(OnSfTreeViewLoaded);
        NodeExpandedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        NodeCollapsedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        AddFolderCommand = new DelegateCommand(OnAddFolder);
        OpenCommand = new DelegateCommand(OnOpenFileCommand);
        EditCommand = new DelegateCommand(OnEdit);
        ItemEndEditCommand = new DelegateCommand<TreeViewItemEndEditEventArgs>(OnItemEndEdit);
        ContextMenuOpenedCommand = new DelegateCommand(OnContextMenuOpened);
        AddEquipmentsFileCommand = new DelegateCommand(OnAddEquipmentsFile);
        /*AddRepairsFileCommand = new DelegateCommand(OnAddRepairsFile);
        AddServicesFileCommand = new DelegateCommand(OnAddServicesFile);
        AddWriteOffFileCommand = new DelegateCommand(OnAddWriteOffFile);*/
        AddEquipmentsSummaryReportCommand = new DelegateCommand(OnAddEquipmentsSummaryFile);
        AddRepairsSummaryReportCommand = new DelegateCommand(OnAddRepairsSummaryFile);
        AddServicesSummaryReportCommand = new DelegateCommand(OnAddServicesSummaryFile);
        AddWriteOffSummaryReportCommand = new DelegateCommand(OnAddWriteOffSummaryFile);
    }

    // Summaries
    private async void OnAddEquipmentsSummaryFile()
    {
        await AddSummaryFile(SummaryFormat.EquipmentsSummary);
    }
    private async void OnAddRepairsSummaryFile()
    {
        await AddSummaryFile(SummaryFormat.RepairsSummary);
    }
    private async void OnAddServicesSummaryFile()
    {
        await AddSummaryFile(SummaryFormat.ServicesSummary);
    }
    private async void OnAddWriteOffSummaryFile()
    {
        await AddSummaryFile(SummaryFormat.WriteOffSummary);
    }
    
    // Equipments
    private async void OnAddEquipmentsFile()
    {
       await AddEquipmentFile(FileFormat.EquipmentSheet);
    }
    
    

    private async Task AddEquipmentFile(FileFormat fileFormat)
    {
        var folder = SelectedItem as FolderItem;
        string baseName = GetBaseNameFromFileFormat(fileFormat);
        var siblingNames = folder.SubItems.OfType<FileItem>().Select(f => f.Name).ToList();
        string uniqueName = await _service.GenerateUniqueFileFolderNameAsync(baseName, siblingNames);

        int tableId = await _service.CreateEquipmentTableAsync();
        
        var newFile = new FileItem
        {
            ParentIdFolder = folder.Id,
            Name = uniqueName,
            TableId = tableId,
            Id = await _service.CreateFileAsync(uniqueName, fileFormat, folder.Id, tableId, _menuType),
            FileFormat = fileFormat,
        };
        folder.AddFile(newFile);
        SelectedItem = newFile;
        OnEdit();
    }
    
    private async Task AddSummaryFile(SummaryFormat summaryFormat)
    {
        var folder = SelectedItem as FolderItem;
        string baseName = GetBaseNameFromSummaryFormat(summaryFormat);
        var siblingNames = folder.SubItems.OfType<FileItem>().Select(f => f.Name).ToList();
        string uniqueName = await _service.GenerateUniqueFileFolderNameAsync(baseName, siblingNames);

        int summaryId = await _service.CreateSummaryAsync(summaryFormat);
        
        var newFile = new FileItem
        {
            ParentIdFolder = folder.Id,
            Name = uniqueName,
            SummaryId = summaryId,
            Id = await _service.CreateSummaryFileAsync(uniqueName, folder.Id, summaryId, _menuType),
            FileFormat = FileFormat.Summary,
        };
        folder.AddFile(newFile);
        SelectedItem = newFile;
        OnEdit();
    }

    private string GetBaseNameFromFileFormat(FileFormat fileFormat)
    {
        switch (fileFormat)
        {
            case FileFormat.EquipmentSheet:
                return "Нове обладнання";
            case FileFormat.RepairsSheet:
                return "Нові ремонти";
            case FileFormat.WriteOffSheet:
                return "Списані";
            case FileFormat.ServicesSheet:
                return "Нові обслуговування";
        }
        return string.Empty;
    }
    
    private string GetBaseNameFromSummaryFormat(SummaryFormat summaryFormat)
    {
        switch (summaryFormat)
        {
            case SummaryFormat.EquipmentsSummary:
                return "Загальний звіт обладнання";
            case SummaryFormat.RepairsSummary:
                return "Загальний звіт ремонти";
            case SummaryFormat.WriteOffSummary:
                return "Загальний звіт списане обладнання";
            case SummaryFormat.ServicesSummary:
                return "Загальний звіт обслуговування";
        }
        return string.Empty;
    }

    private void RaiseContextMenuItemsVisibilityChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(OpenContextMenuVisibility)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ConnectEquipmentContextMenuVisibility)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ConnectionsContextMenuVisibility)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(CreateContextMenuItemVisibility)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(EditContextMenuItemVisibility)));
    }

    private void OnOpenFileCommand()
    {
        if (SelectedItem is FileItem fileItem)
        {
            string viewName = string.Empty;
            switch (fileItem.FileFormat)
            {
                case FileFormat.EquipmentSheet:
                    viewName = "EquipmentDataGridView";
                    break;

                case FileFormat.WriteOffSheet:
                    viewName = "WriteoffDataGridView";
                    break;

                case FileFormat.ServicesSheet:
                    viewName = "ServicesDataGridView";
                    break;

                case FileFormat.RepairsSheet:
                    viewName = "RepairsDataGridView";
                    break;
                case FileFormat.Summary:
                    viewName = "SummarySheetView";
                    break;
            }
            
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = fileItem.Name,
                Parameters = new Dictionary<string, object>
                {
                    { "ViewNameToShow", viewName },
                    { "EquipmentDataGridView.TableId", fileItem.TableId },
                    { "RepairsDataGridView.TableId", fileItem.TableId },
                    { "ServicesDataGridView.TableId", fileItem.TableId },
                    { "SummarySheetView.SummaryId", fileItem.SummaryId },
                }
            });
        }
    }
    
    private string _oldFileSystemItemName;
    private void OnEdit()
    {
        var node = FindNodeByContent(_sfTreeView.Nodes, SelectedItem);
        if (node != null)
        {
            _sfTreeView.BeginEdit(node);
            if (SelectedItem is IFileSystemItem fileSystemItem)
            {
                _oldFileSystemItemName = fileSystemItem.Name;
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
        if (args.Node.Content is not IFileSystemItem editedItem) return;

        string oldName = _oldFileSystemItemName;
        string requestedName = editedItem.Name;
        
        if (!ValidateName(requestedName, oldName))
        {
            editedItem.Name = oldName;
            return;
        }
        
        var parentNode = args.Node.ParentNode;
        var siblingNames = parentNode?.ChildNodes
            .Select(n => (IFileSystemItem)n.Content)
            .Where(it => !ReferenceEquals(it, editedItem))
            .Select(it => it.Name)
            .ToList() ?? new List<string>();

        string uniqueName = await _service.GenerateUniqueFileFolderNameAsync(requestedName, siblingNames);

        if (uniqueName == oldName)
            return;
        
        editedItem.Name = uniqueName;
        
        if (editedItem is FileItem file)
            await _service.RenameFileAsync(file.Id, uniqueName);
        else if (editedItem is FolderItem folder)
            await _service.RenameFolderAsync(folder.Id, uniqueName);
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
        Items = await _service.BuildHierarchy(_menuType);
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
        int newId = await _service.CreateFolderAsync(uniqueName, parentId, _menuType);
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
    }
    
   
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
        {
            _regionManager = scopedRegionManager;
        }
        _menuType = navigationContext.Parameters.GetValue<MenuType>("MenuType");
        LoadTreeAsync();
    }
    

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        Console.WriteLine("OnNavigatedFrom");
    }

    public bool KeepAlive => false;
    public void Destroy()
    {
        Console.WriteLine("Destroy");
    }
}