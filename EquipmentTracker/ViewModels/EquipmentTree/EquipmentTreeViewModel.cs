using System.Collections.ObjectModel;
using System.ComponentModel;
using Common.Logging;
using Core.Events.TabControl;
using Core.Services.EquipmentTree;
using EquipmentTracker.Constants.Common;
using EquipmentTracker.Constants.Equipment;
using Syncfusion.UI.Xaml.TreeView;
using MaterialDesignThemes.Wpf;
using Models.Constants;
using Models.EquipmentTree;
using Models.Enums;
using Notification.Wpf;
using Syncfusion.UI.Xaml.TreeView.Engine;
using UI.ViewModels.TabControl;
using DelegateCommand = Prism.Commands.DelegateCommand;

namespace UI.ViewModels.EquipmentTree;

public class EquipmentTreeViewModel : BindableBase, INavigationAware, IRegionMemberLifetime, IDestructible
{
    #region Properties
    private IEquipmentTreeService _service;
    private readonly IEventAggregator _eventAggregator;
    private readonly IAppLogger<EquipmentTreeViewModel> _logger;
    private readonly NotificationManager _notificationManager;
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
    public EquipmentTreeViewModel(IEventAggregator eventAggregator, IAppLogger<EquipmentTreeViewModel> logger, NotificationManager notificationManager)
    {
        _scopedEventAggregator = new EventAggregator();
        _eventAggregator = eventAggregator;
        _logger = logger;
        _notificationManager = notificationManager;

        SfTreeViewLoadedCommand = new DelegateCommand<SfTreeView>(OnSfTreeViewLoaded);
        NodeExpandedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        NodeCollapsedCommand = new DelegateCommand<NodeExpandedCollapsedEventArgs>(OnNodeExpandedCollapsed);
        AddFolderCommand = new DelegateCommand(async () => await OnAddFolder());
        OpenCommand = new DelegateCommand(OnOpenFileCommand);
        EditCommand = new DelegateCommand(OnEdit);
        ItemEndEditCommand = new DelegateCommand<TreeViewItemEndEditEventArgs>(OnItemEndEdit);
        ContextMenuOpenedCommand = new DelegateCommand(OnContextMenuOpened);
        AddEquipmentsFileCommand = new DelegateCommand(OnAddEquipmentsFile);
        AddEquipmentsSummaryReportCommand = new DelegateCommand(OnAddEquipmentsSummaryFile);
        AddRepairsSummaryReportCommand = new DelegateCommand(OnAddRepairsSummaryFile);
        AddServicesSummaryReportCommand = new DelegateCommand(OnAddServicesSummaryFile);
        AddWriteOffSummaryReportCommand = new DelegateCommand(OnAddWriteOffSummaryFile);
    }
    
    private async void OnAddEquipmentsSummaryFile()
    {
        await OnAddFile(FileFormat.SummaryEquipment);
    }
    private async void OnAddRepairsSummaryFile()
    {
        await OnAddFile(FileFormat.SummaryRepairs);
    }
    private async void OnAddServicesSummaryFile()
    {
        await OnAddFile(FileFormat.SummaryServices);
    }
    private async void OnAddWriteOffSummaryFile()
    {
        await OnAddFile(FileFormat.SummaryWriteOff);
    }
    
    private async void OnAddEquipmentsFile()
    {
        await OnAddFile(FileFormat.EquipmentSheet);
    }

    private async Task OnAddFile(FileFormat fileFormat)
    {
        var folder = SelectedItem as FolderItem;
        string baseName = GetBaseNameFromFileFormat(fileFormat);
        var siblingNames = folder.SubItems.OfType<FileItem>().Select(f => f.Name).ToList();
        string uniqueName = _service.GenerateUniqueFileFolderName(baseName, siblingNames);
        Guid id = Guid.NewGuid();
        var newFile = new FileItem
        {
            Id = id,
            FolderId = folder.Id,
            Name = uniqueName,
            FileFormat = fileFormat,
            MenuType = _menuType,
        };

        using var cts = new CancellationTokenSource();
        try
        {
            await _service.CreateFileAsync(newFile, fileFormat, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _notificationManager.Show("Створення файлу скасовано", NotificationType.Warning);
            _logger.LogWarning("File creation was cancelled.");
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка створення файлу", NotificationType.Error);
            _logger.LogError($"Failed to create file: {e.Message}");
            throw;
        }
        
        folder.AddFile(newFile);
        SelectedItem = newFile;
        OnEdit();
    }
    
    private async Task OnAddFolder()
    {
        Guid? folderId = SelectedItem is FolderItem folderItem ? folderItem.Id : null;
        string baseName = "Нова папка";
        var folderNames = GetAllFolderNames(Items).ToList();
        string uniqueName = _service.GenerateUniqueFileFolderName(baseName, folderNames);
        Guid newId = Guid.NewGuid();
        var newFolder = new FolderItem
        {
            Name = uniqueName,
            Id = newId,
            MenuType = _menuType,
            FolderId = folderId,
        };
        

        if (SelectedItem is FolderItem parentFolder)
        {
            parentFolder.AddFolder(newFolder);
            ExpandFolder(newFolder);
        }
        else
        {
            Items.Add(newFolder);
        }
        
        using var cts = new CancellationTokenSource();

        try
        {
            await _service.CreateFolderAsync(newFolder, cts.Token);
        }
        catch (OperationCanceledException)
        {
            _notificationManager.Show("Додавання папки скасовано");
            _logger.LogError("Creation of folder was cancelled.");
        }
        catch (Exception e)
        {
            _notificationManager.Show("Помилка додавання папки");
            _logger.LogError($"Failed to create folder: {e.Message}");
            throw;
        }
        
        SelectedItem = newFolder;
        
        OnEdit();
        CheckEmptyData();
    }

    private void ExpandFolder(FolderItem folder)
    {
        var parentNode = FindTreeNode(_sfTreeView, folder);
        if (!parentNode.IsExpanded)
        {
            _sfTreeView.ExpandNode(parentNode);
            if (parentNode.Content is FolderItem folderItem)
            {
                folderItem.ImageIcon = "Assets/opened_folder.png";
            }
        }
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
            case FileFormat.SummaryEquipment:
                return "Новий загальний лист обладннання";
            case FileFormat.SummaryRepairs:
                return "Новий загальний лист ремонтів";
            case FileFormat.SummaryServices:
                return "Новий загальний лист обслуговування";
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
                    viewName = "EquipmentSheetView";
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
                case FileFormat.SummaryEquipment:
                    viewName = "SummarySheetView";
                    break;
            }
            
            Console.WriteLine(fileItem.TableId);
            
            _eventAggregator.GetEvent<OpenNewTabEvent>().Publish(new OpenNewTabEventArgs
            {
                Header = fileItem.Name,
                Parameters = new Dictionary<string, object>
                {
                    { "ViewNameToShow", viewName },
                    { $"EquipmentSheetView.{EquipmentSheetConstants.EquipmentSheetId}", fileItem.EquipmentSheetId },
                    { "RepairsDataGridView.TableId", fileItem.TableId },
                    { "ServicesDataGridView.TableId", fileItem.TableId },
                    { "SummarySheetView.SummaryId", fileItem.SummaryId },
                    { "SummarySheetView.SummaryName", fileItem.Name },
                    {$"{ViewNameConstants.EquipmentDataGridView}.{EquipmentConstants.EquipmentSheetName}",fileItem.Name}
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

        string uniqueName = _service.GenerateUniqueFileFolderName(requestedName, siblingNames);

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


    private bool _isInitialized = false;
    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (!_isInitialized)
        {
            var tabScopedServiceProvider = navigationContext.Parameters.GetValue<IScopedProvider>("TabScopedServiceProvider");
            _service = tabScopedServiceProvider.Resolve<IEquipmentTreeService>();
            if (navigationContext.Parameters["ScopedRegionManager"] is IRegionManager scopedRegionManager)
            {
                _regionManager = scopedRegionManager;
            }
            _menuType = navigationContext.Parameters.GetValue<MenuType>("MenuType");
            LoadTreeAsync();
            _isInitialized = true;
        }
    }
    

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
        Console.WriteLine("OnNavigatedFrom");
    }

    public bool KeepAlive => true;
    public void Destroy()
    {
        Console.WriteLine("Destroy");
    }
}