using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using Data.AppDbContext;
using Data.Entities;

using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;

using Core.Models.Tabs.ProductionEquipmentTree;
using Core.Services.Log;
using Core.Services.Notifications;
using Notification.Wpf;
using Ookii.Dialogs.Wpf;
using TaskDialog = Ookii.Dialogs.Wpf.TaskDialog;
using TaskDialogButton = Ookii.Dialogs.Wpf.TaskDialogButton;

using DelegateCommand = Prism.Commands.DelegateCommand;



namespace UI.ViewModels.Tabs;

public class EquipmentTreeViewModel : BindableBase
{
    #region Properties
    private readonly AppDbContext _context;
    private readonly IEventAggregator _eventAggregator;
    private readonly BusyIndicatorService _busyIndicatorService;
    private readonly LogService _logService;
    private readonly NotificationManager _notificationManager;
    private ObservableCollection<Folder> _folders;
    
    private string _originalCategoryName;
    private EquipmentTreeModel _model;
    private int? _originalCategoryId;
    private SfTreeView treeView;
    private Folder _selectedFolder;
    #endregion

    
    
    #region Commands
    public DelegateCommand<object> OpenFileCommand { get; }
    public DelegateCommand AddCategoryCommand { get; }
    public DelegateCommand<object> AddSubCategoryCommand { get; }
    public DelegateCommand<object> DeleteCommand { get; }
    public DelegateCommand<object> ClearSelectionCommand { get; }
    public DelegateCommand EditCommand { get; }
    private DelegateCommand<TreeViewItemBeginEditEventArgs> _itemBeginEditCommand;
    private DelegateCommand<TreeViewItemEndEditEventArgs> _itemEndEditCommand;
    public DelegateCommand<TreeViewItemBeginEditEventArgs> ItemBeginEditCommand =>
        _itemBeginEditCommand ??= new DelegateCommand<TreeViewItemBeginEditEventArgs>(ExecuteItemBeginEdit);

    public DelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand =>
        _itemEndEditCommand ??= new DelegateCommand<TreeViewItemEndEditEventArgs>(ExecuteItemEndEdit);
    
    #endregion 
    
    

    #region Services
    public bool CanAddSubCategory => SelectedFolder != null;
    public bool CanEdit => SelectedFolder != null;
    public bool CanDeleteCategory => SelectedFolder != null;
    public ObservableCollection<Folder> Folders
    {
        get => _folders;
        set => SetProperty(ref _folders, value);
    }

    public SfTreeView TreeView
    {
        get => treeView;
        set => SetProperty(ref treeView, value);
    }

    public Folder SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            if (SetProperty(ref _selectedFolder, value))
            {
                RaisePropertyChanged(nameof(CanAddSubCategory));
                RaisePropertyChanged(nameof(CanEdit));
                RaisePropertyChanged(nameof(CanDeleteCategory));
            }
        }
    }
    #endregion
    
    
    
    #region Initialization
    public EquipmentTreeViewModel(AppDbContext context, IEventAggregator eventAggregator, BusyIndicatorService busyIndicatorService, EquipmentTreeModel model, LogService logService, NotificationManager notificationManager, SfTreeView _treeView)
    {
        AddCategoryCommand = new DelegateCommand(OnCreate);
        AddSubCategoryCommand = new DelegateCommand<object>(OnCreateSubCategory);
        OpenFileCommand = new DelegateCommand<object>(OpenFile);
        DeleteCommand = new DelegateCommand<object>(OnDelete);
        EditCommand = new DelegateCommand(OnEdit);

        
        
        _context = context;
        _eventAggregator = eventAggregator;
        _busyIndicatorService = busyIndicatorService;
        _model = model;
        _logService = logService;
        _notificationManager = notificationManager;
        
        LoadData();
    }
    #endregion
    
    
    #region Open file in tab
    private void OpenFile(object item)
    {
        var selectedItem = item as File;

        if (selectedItem != null)
        {
            _eventAggregator.GetEvent<Core.Events.TabControl.OpenTabEvent>().Publish(selectedItem.FileName);
        }
    }
    #endregion
    
    #region LoadData
    private void LoadData()
    {
        var expandedNodes = SaveExpandedNodes();
        
        var categories = _context.CategoriesProductionEquipment
            .AsNoTracking()
            .ToList();

       Folders = new ObservableCollection<Folder>(BuildFolderHierarchy(categories, null));
       
       RestoreExpandedNodes(expandedNodes);
    }
    #endregion
    
    
    
    #region BuildForHierarchy
    private IEnumerable<Folder> BuildFolderHierarchy(List<CategoryProductionEquipment> categories, int? parentId)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.CategoryName,
                Comparer<string>.Create((a, b) =>
                    string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase))) // Sorting
            .Select(c => new Folder
            {
                Id = c.Id,
                FileName = c.CategoryName,
                SubFolders = BuildFolderHierarchy(categories, c.Id).ToList(),
            });
    }
    #endregion
    
    #region SaveExpandedNodes
    private List<string> SaveExpandedNodes()
    {
        var expanded = new List<string>();
        if (treeView != null)
        {
            foreach (var node in treeView.Nodes)
            {
                CollectExpandedNodes(node, expanded);
            }
        }
        return expanded;
    }

    private void CollectExpandedNodes(TreeViewNode node, List<string> expanded)
    {
        if (node.IsExpanded && node.Content is Folder folder)
        {
            expanded.Add(folder.FileName);
        }

        foreach (var childNodes in node.ChildNodes)
        {
            CollectExpandedNodes(childNodes, expanded);
        }
    }
#endregion
    #region RestoreExpandedNodes
    private void RestoreExpandedNodes(List<string> expandedNodes)
    {
        if (treeView != null)
        {
            foreach (var node in treeView.Nodes)
            {
                RestoreNodeState(node, expandedNodes);
            }
        }
    }

    private void RestoreNodeState(TreeViewNode node, List<string> expandedNodes)
    {
        if (node.Content is Folder folder && expandedNodes.Contains(folder.FileName))
        {
            node.IsExpanded = true;
        }

        foreach (var childNode in node.ChildNodes)
        {
            RestoreNodeState(childNode, expandedNodes);
        }
    }
    #endregion
    
    
    
    #region Get Unique Category Name
    //For creatings category
    private string GetUniqueCategoryName(string baseName, int? parentId = null)
    {
        string newName = baseName;
        int counter = 1;
        
        while (_context.CategoriesProductionEquipment.Any(c => c.CategoryName == newName && c.ParentId == parentId))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }
        return newName;
    }
    //For editings
    private string GetUniqueCategoryName(string baseName, string originalName, int? parentId = null)
    {
        string newName = baseName;
        int counter = 1;

        while (_context.CategoriesProductionEquipment.Any(c => c.CategoryName == newName && c.CategoryName != originalName && c.ParentId == parentId))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }
        return newName;
    }
    #endregion
    
    #region CreateCategory (creation and editing at once)
    private void OnCreate()
    {
        string message;
        bool isCreated = _model.Creating(out message);
        if (isCreated)
        {
            LoadData();
            _notificationManager.Show("", message, NotificationType.Information);
        }
        else
        {
            _notificationManager.Show("", message, NotificationType.Information);
        }
    }
    
#endregion
#region Blur
private void BlurBackground()
{
    _busyIndicatorService.StartBusy();
    _busyIndicatorService.HiddenBusyIndicator();
    _busyIndicatorService.EmptyMessage();
}
private void StopBlurBackground()
{
    _busyIndicatorService.StopBusy();
}
#endregion

    #region On create sub category
    private void OnCreateSubCategory(object item)
    {
        string message;
        bool isCreated = _model.CreatingSubCategory(item, out message);
        
        if (isCreated)
        {
            LoadData();
            _notificationManager.Show("", message, NotificationType.Information);
        }
        else
        {
            _notificationManager.Show("", message, NotificationType.Information);
        }
    }
#endregion

    #region On Delete
    private void OnDelete(object item)
    {
        if (item is Folder folderToDelete)
        {
            BlurBackground();

            var dialog = new TaskDialog
            {
                WindowTitle = "Підтвердження",
                MainInstruction = "Ви впевнені, що хочете видалити цю категорію?",
                Content = folderToDelete.FileName,
                Buttons = { new TaskDialogButton(ButtonType.Yes), new TaskDialogButton(ButtonType.No) }
            };
            
            var result = dialog.Show();
            StopBlurBackground();

            if (result.ButtonType == ButtonType.Yes)
            {
                string message;
                bool isDeleted = _model.Deleting(folderToDelete.Id, out message);
                
                if (isDeleted)
                {
                    treeView.SelectedItem = null;
                    LoadData();
                    _notificationManager.Show("", message, NotificationType.Information);
                }
            }
        }
        else
        {
            _notificationManager.Show("", "Не вибрано категорію", NotificationType.Information);
        }
    }
    #endregion
    #region OnEdit
    private void OnEdit()
    {
      _model.F2Imitation();
    }
    #endregion
    
    #region Editing tree items
    #region ExecuteItemBeginEdit
    private void ExecuteItemBeginEdit(TreeViewItemBeginEditEventArgs e)
    {
        var editedItem = e.Node.Content as Folder;
        if (editedItem != null)
        {
            _model.BeginEditing(editedItem);
        }
    }
    #endregion
    #region ExecuteItemEndEdit
    private void ExecuteItemEndEdit(TreeViewItemEndEditEventArgs e)
    {
       var editedItem = e.Node.Content as Folder;
       if (editedItem == null)return;
       
       string message;
       if (_model.EndEditing(editedItem, out message))
       {
           LoadData();
           SelectedFolder = null;
           _notificationManager.Show("", message, NotificationType.Information);
       }
       else
       {
           _notificationManager.Show("", message, NotificationType.Error);
       }

    }
    #endregion
    #endregion
}