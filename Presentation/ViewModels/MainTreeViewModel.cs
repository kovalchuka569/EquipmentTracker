using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using Prism.Events;
using Prism.Commands;
using Prism.Navigation.Regions;
using Syncfusion.UI.Xaml.TreeView;
using Notification.Wpf;
using Core.Interfaces;
using Presentation.ViewModels.Common.FileSystem;
using Presentation.Mappers;
using Common.Enums;
using Common.Logging;
using Core.Events.TabControl;
using Presentation.ViewModels.Common;
using Unity;

namespace Presentation.ViewModels;

public class MainTreeViewModel : InteractiveViewModelBase, IRegionMemberLifetime
{
    #region Constants
    
    private const string LoadItemsErrorMessageUi = "Виникла помилка під час завантаження даних.";
    private const string LoadItemsErrorMessageLogger = "Error while loading items.";
    private const string InsertItemErrorMessageUi = "Виникла помилка під час додавання елементу.";
    private const string InsertItemErrorMessageLogger = "Error while inserting root item.";
    private const string ItemEditErrorMessageUi = "Виникла помилка під час редагування елементу.";
    private const string ItemEditErrorMessageLogger = "Error while editing root item.";
    private const string DraggedItemsErrorMessageUi = "Виникла помилка під час зміни розташування елемента.";
    private const string DraggedItemsErrorMessageLogger = "Error while dragging root items.";
    private const int ChildsLoadingDelay = 150;
    
    #endregion
    
    #region Dependencies
    
    [Dependency]
    public required IAppLogger<MainTreeViewModel> Logger { get; init; } = null!;
    
    [Dependency]
    public required IEventAggregator EventAggregator { get; init; } = null!;
    
    [Dependency]
    public required IFileSystemService FileSystemService { get; init; } = null!;
    
    [Dependency]
    public required NotificationManager NotificationManager { get; init; } = null!;
    
    #endregion
    
    #region Private fields
    
    private bool _isInitialized;
    
    private MenuType _menuType = MenuType.None;
    
    private ObservableCollection<FileSystemItemBaseViewModel> _rootItems = new();
    
    private ObservableCollection<object> _selectedItems = new();

    private bool _filesEmptyTipVisibility;
    
    private object _editItem = new();
    
    #endregion
    
    #region Public fields
    
    public ObservableCollection<FileSystemItemBaseViewModel> RootItems
    {
        get => _rootItems;
        set => SetProperty(ref _rootItems, value);
    }

    public ObservableCollection<object> SelectedItems
    {
        get => _selectedItems;
        set => SetProperty(ref _selectedItems, value);
    }

    public bool FilesEmptyTipVisibility
    {
        get => _filesEmptyTipVisibility;
        set => SetProperty(ref _filesEmptyTipVisibility, value);
    }

    public object EditItem
    {
        get => _editItem;
        set => SetProperty(ref _editItem, value);
    }
    
    #endregion
    
    #region Constructor
    
    public MainTreeViewModel()
    {
        InitializeCommands();
    }
    
    #endregion
    
    #region Commands
    public AsyncDelegateCommand TreeViewLoadedCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<object> AddFileContextMenuCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand EditItemContextMenuCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<NodeExpandingCollapsingEventArgs> NodeExpandingCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<TreeViewItemDroppedEventArgs> ItemDroppedCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<object> ItemTappedCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand<KeyEventArgs> KeyDownCommand { [UsedImplicitly] get; set; } = null!;
    public AsyncDelegateCommand RefreshTreeCommand { [UsedImplicitly] get; set; } = null!;
    
    #endregion
    
    #region Command Management

    private void InitializeCommands()
    {
        TreeViewLoadedCommand = new AsyncDelegateCommand(OnTreeViewLoaded);
        AddFileContextMenuCommand = new AsyncDelegateCommand<object>(OnAddFile);
        EditItemContextMenuCommand = new AsyncDelegateCommand(OnEditItem);
        KeyDownCommand = new AsyncDelegateCommand<KeyEventArgs>(OnKeyDown);
        ItemTappedCommand = new AsyncDelegateCommand<object>(OnOpenFile);
        ItemEndEditCommand = new AsyncDelegateCommand<TreeViewItemEndEditEventArgs>(OnItemEndEdit);
        NodeExpandingCommand = new AsyncDelegateCommand<NodeExpandingCollapsingEventArgs>(OnItemExpandingCommand);
        ItemDroppedCommand = new AsyncDelegateCommand<TreeViewItemDroppedEventArgs>(OnItemDropped);
        RefreshTreeCommand = new AsyncDelegateCommand(RefreshTree);
    }
    
    #endregion
    
    #region Private methods

    private async Task OnTreeViewLoaded()
    {
        // Check is initialized, if not return.
        if(_isInitialized)
            return;
        
        // Load root items
        await LoadRootItems();
    }

    private async Task LoadRootItems()
    {
        await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Get root items.
            var rootFileItems = await FileSystemService.GetChildsAsync(_menuType, null);

            // Sort by order.
            var sortedRootFileItems = rootFileItems.OrderBy(x => x.Order);

            // Adding in UI collection.
            foreach (var domain in sortedRootFileItems)
                RootItems.Add(FileSystemMapper.ToViewModel(domain));

            // Mark data loading as completed         
            _isInitialized = true;
            
            // Raise files empty tip visibility
            RaiseEmptyTipVisibility();
            
        }, onError: e =>
        {
            Logger.LogError(e, LoadItemsErrorMessageLogger);
            NotificationManager.Show(LoadItemsErrorMessageUi, NotificationType.Error);
        });
    }
    
    private Task OnKeyDown(KeyEventArgs args)
    {
        if (args.Key == Key.Escape)
        {
            SelectedItems.Clear();
        }
        return Task.CompletedTask;
    }

    private Task OnEditItem()
    {
        if (SelectedItems.Count > 0)
            EditItem = SelectedItems.First();
        return Task.CompletedTask;
    }

    private async Task OnAddFile(object parameter)
    {
        // Check parameter is file format, if not return.
        if (parameter is not FileFormat fileFormat) 
            return;
        
        await ExecuteWithErrorHandlingAsync(async () => 
        { 
            var newItem = FileSystemMapper.FileFormatToViewModel(fileFormat);
            
            newItem.Name = FileSystemMapper.FileFormatToNewName(fileFormat);
            newItem.MenuType = _menuType;
            
            // Create new ID for sheets
            switch (newItem)
            {
                case EquipmentSheetFileViewModel ef:
                    ef.EquipmentSheetId = Guid.NewGuid(); 
                    break;
                case PivotSheetFileViewModel pf:
                    pf.PivotSheetId = Guid.NewGuid();
                    break;
            }

            // If we have selected item, get first or default and add child
            if (SelectedItems.FirstOrDefault() is FileSystemItemBaseViewModel parentItem)
            {
                newItem.Parent = parentItem;
                newItem.ParentId = parentItem.Id;
                newItem.Order = parentItem.Childs.Count;
                parentItem.Childs.Add(newItem);
                parentItem.HasChilds = true;
                parentItem.IsExpanded = true;
                
                await FileSystemService.UpdateHasChildsAsync(parentItem.Id, true);
            }
            // Else add in root
            else
            {
                newItem.Order = RootItems.Count;
                RootItems.Add(newItem);
            }
            
            // Map to domain
            var newItemDomain = FileSystemMapper.ToDomain(newItem);

            // Persist in DB
            await FileSystemService.InsertChildAsync(newItemDomain);
            
            // Raise files empty tip visibility
           RaiseEmptyTipVisibility();
            
        }, onError: e =>
        {
            Logger.LogError(e, InsertItemErrorMessageLogger);
            NotificationManager.Show(InsertItemErrorMessageUi, NotificationType.Error);
        });
    }

    private Task OnOpenFile(object parameter)
    {
        if(parameter is not FileSystemItemBaseViewModel baseItem)
            return Task.CompletedTask;
            
        var viewName = FileSystemMapper.FileFormatToViewName(baseItem.Format);
            
        switch (baseItem)
        {
            case EquipmentSheetFileViewModel ef:
                EventAggregator.GetEvent<OpenNewTabEvent>()
                    .Publish(GetOpenFileEventArgs(baseItem.Name, viewName, "EquipmentSheetId", ef.EquipmentSheetId));
                break;
            case PivotSheetFileViewModel pf:
                EventAggregator.GetEvent<OpenNewTabEvent>()
                        .Publish(GetOpenFileEventArgs(baseItem.Name, viewName, "PivotSheetId", pf.PivotSheetId));
                break;
        }

        return Task.CompletedTask;
    }

    private static OpenNewTabEventArgs GetOpenFileEventArgs(string tabName, string viewName, string parameterName, object? parameterValue)
    {
        return new OpenNewTabEventArgs
        {
            Header = tabName,
            Parameters = new Dictionary<string, object?>
            {
                { "ViewNameToShow", viewName },
                { $"{viewName}.{parameterName}", parameterValue }
            }
        };
    }

    private async Task OnItemEndEdit(TreeViewItemEndEditEventArgs args)
    {
        // Check the edited item is file system item view model.
        if (args.Node.Content is not FileSystemItemBaseViewModel item) 
            return;

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Check is valid item, if not return.
            if (!item.IsValid)
            {
                args.Cancel = true;
                return;
            }
            
            await FileSystemService.RenameFileSystemItemAsync(item.Id, item.Name);
        }, onError: e =>
        {
            Logger.LogError(e, ItemEditErrorMessageLogger);
            NotificationManager.Show(ItemEditErrorMessageUi, NotificationType.Error);
        });
    }
    
    private async Task OnItemExpandingCommand(NodeExpandingCollapsingEventArgs args)
    {
        if(args.Node.Content is not FileSystemItemBaseViewModel item)
            return;
        
        if(item.IsChildsLoaded)
            return;

        await LoadChildsAsync(item);
    }

    private async Task OnItemDropped(TreeViewItemDroppedEventArgs args)
    {
        // Lists to store changes for updating database
        var changes = new List<(Guid Id, Guid? ParentId, int Order)>();
        var hasChildrenChanges = new List<(Guid Id, bool HasChildren)>();
        var affectedParents = new HashSet<FileSystemItemBaseViewModel>();

        // 1. Check target node
        if (args.TargetNode.Content is not FileSystemItemBaseViewModel targetNode)
            return;

        // 2. If the parent has not loaded children, we load them
        if (!targetNode.IsChildsLoaded)
        {
            await LoadChildsAsync(targetNode);
        }
        
        // 3. Determine the new parent (null if dropped at root level)
        var newParent = args.DropPosition == DropPosition.DropAsChild ? targetNode : targetNode.Parent;
        
        // 4. Update Parent and ParentId for all dragged items
        foreach (var node in args.DraggingNodes)
        {
            if (node.Content is not FileSystemItemBaseViewModel item)
                continue;

            var oldParent = item.Parent;
            if (oldParent != null)
                affectedParents.Add(oldParent); // Keep track of old parents for reindexing

            item.Parent = newParent;          // Can be null if moved to root
            item.ParentId = newParent?.Id;    // Set to null if at root level
        }

        // 5. Add the new parent to affected parents set for reindexing
        if (newParent != null)
            affectedParents.Add(newParent);
        
        // 6. Reindex all affected parents and update HasChilds flags
        foreach (var parent in affectedParents)
        {
            
            changes.AddRange(parent.ReindexChildSiblings()); // Update order of children

            var hasChildren = parent.Childs.Count > 0;
            if (parent.HasChilds != hasChildren)
            {
                parent.HasChilds = hasChildren;
                parent.IsExpanded = hasChildren && parent.IsExpanded;
                hasChildrenChanges.Add((parent.Id, hasChildren)); // Track HasChilds changes
            }
        }

        // 7. Handle items moved to the root level separately
        var rootItemsChanged = args.DraggingNodes
            .Select(n => n.Content as FileSystemItemBaseViewModel)
            .Where(i => i is { Parent: null })
            .ToList();

        if (rootItemsChanged.Count > 0)
        {
            // Add to RootItems collection if not already present
            foreach (var item in rootItemsChanged)
            {
                if (item != null && !RootItems.Contains(item))
                    RootItems.Add(item);
            }

            // Reindex root items
            changes.AddRange(ReindexRootItems());
        }

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            // 8. Persist all order and parent changes to the database
            if (changes.Count != 0)
                await FileSystemService.UpdateParentsAndOrdersAsync(changes);

            // 9. Persist any HasChilds changes to the database
            if (hasChildrenChanges.Count != 0)
                await FileSystemService.UpdateHasChildsAsync(hasChildrenChanges);
        }, e =>
        {
            Logger.LogError(e, DraggedItemsErrorMessageLogger);
            NotificationManager.Show(DraggedItemsErrorMessageUi, NotificationType.Error);
        });
    }

    private async Task LoadChildsAsync(FileSystemItemBaseViewModel item)
    {
        if (item.IsChildsLoaded)
            return;
        
        item.IsLoading = true;

        await ExecuteWithErrorHandlingAsync(async () =>
        {
            // Remove dummy children
            foreach (var dummy in item.Childs.OfType<DummyFileViewModel>().ToList())
                item.Childs.Remove(dummy);

            // Load from DB
            var childsFromDb = await FileSystemService.GetChildsAsync(_menuType, item.Id);
            var sortedChilds = childsFromDb.OrderBy(c => c.Order).ToList();

            var existing = item.Childs.ToDictionary(c => c.Id);
            var loadedIds = new HashSet<Guid>(sortedChilds.Select(c => c.Id));

            item.Childs.Clear();
            foreach (var childModel in sortedChilds)
            {
                if (existing.TryGetValue(childModel.Id, out var existingVm))
                    item.Childs.Add(existingVm);
                else
                {
                    var childVm = FileSystemMapper.ToViewModel(childModel);
                    childVm.Parent = item;
                    item.Childs.Add(childVm);
                }
            }
            
            await Task.Delay(ChildsLoadingDelay);

            // Add orphaned local children (not in DB)
            foreach (var orphan in existing.Values.Where(c => !loadedIds.Contains(c.Id)))
                item.Childs.Add(orphan);

            item.SetChildsLoaded(true);

            item.IsExpanded = true;
        },
        e =>
        {
            item.IsLoading = false;
            Logger.LogError(e, DraggedItemsErrorMessageLogger);
            NotificationManager.Show(DraggedItemsErrorMessageUi, NotificationType.Error);
        },
        onFinally: () => item.IsLoading = false);
    }
    
    private List<(Guid Id, Guid? ParentId, int Order)> ReindexRootItems()
    {
        var result = new List<(Guid Id, Guid? ParentId, int Order)>();

        // Assign sequential order to each root item
        for (int i = 0; i < RootItems.Count; i++)
        {
            var item = RootItems[i];
            item.Order = i;
            item.Parent = null;      
            item.ParentId = null;
            result.Add((item.Id, item.ParentId, item.Order));
        }

        return result;
    }

    private void RaiseEmptyTipVisibility()
    {
        FilesEmptyTipVisibility = RootItems.Count == 0;
    }

    private async Task RefreshTree()
    {
        if (RootItems.Count == 0)
            return;

        // Clear all items
        RootItems.Clear();
        
        // Load roots
        await LoadRootItems();
    }
    
    #endregion
    
    #region Navigation
    
    public bool KeepAlive => true;
    
    public override void OnNavigatedTo(NavigationContext navigationContext)
    {
        base.OnNavigatedTo(navigationContext);
        
        if(_isInitialized)
            return;
        
        _menuType = navigationContext.Parameters.GetValue<MenuType>("MenuType");
    }
    
    #endregion
}