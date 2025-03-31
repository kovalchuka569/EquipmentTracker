using System.Collections.ObjectModel;
using System.ComponentModel;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;

using Notification.Wpf;
using Ookii.Dialogs.Wpf;
using TaskDialog = Ookii.Dialogs.Wpf.TaskDialog;
using TaskDialogButton = Ookii.Dialogs.Wpf.TaskDialogButton;

using Data.Entities;
using Core.Models.Tabs.ProductionEquipmentTree;
using Core.Services.Notifications;
using Syncfusion.Linq;
using Syncfusion.UI.Xaml.Diagram;
using DelegateCommand = Prism.Commands.DelegateCommand;
using UI.Views.Tabs.EquipmentTree;

namespace UI.ViewModels.Tabs;

public class EquipmentTreeViewModel : BindableBase, INavigationAware
{
    #region Properties
    private readonly IEventAggregator _eventAggregator;
    private readonly NotificationManager _notificationManager;
    
    private EquipmentTreeModel _model;
    private SfTreeView treeView;


    private string _menuType;
    private string _editedName;
    #endregion

    
    
    #region Commands
    public DelegateCommand<object> OpenFileCommand { get; }
    public DelegateCommand AddCategoryCommand { get; }
    public DelegateCommand AddFileCommand { get; }
    public DelegateCommand EditCommand { get; }
    private DelegateCommand<TreeViewItemBeginEditEventArgs> _itemBeginEditCommand;
    private DelegateCommand<TreeViewItemEndEditEventArgs> _itemEndEditCommand;
    public DelegateCommand<TreeViewItemBeginEditEventArgs> ItemBeginEditCommand =>
        _itemBeginEditCommand ??= new DelegateCommand<TreeViewItemBeginEditEventArgs>(ExecuteItemBeginEdit);

    public DelegateCommand<TreeViewItemEndEditEventArgs> ItemEndEditCommand =>
        _itemEndEditCommand ??= new DelegateCommand<TreeViewItemEndEditEventArgs>(async (args) => await ExecuteItemEndEditAsync());
    
    #endregion 
    
    

    #region Services

    private ObservableCollection<Folder> _folders;
    private ObservableCollection<File> _files;
    private Folder _selectedFolder;
    private File _selectedFile;

    public ObservableCollection<Folder> Folders
    {
        get => _folders;
        set
        {
            _folders = value;
            RaisePropertyChanged();
        }
    }

    public ObservableCollection<File> Files
    {
        get => _files;
        set
        {
            _files = value;
            RaisePropertyChanged();
        }
    }

    public Folder SelectedFolder
    {
        get => _selectedFolder;
        set
        {
            _selectedFolder = value;
            RaisePropertyChanged();
        }
    }

    public File SelectedFile
    {
        get => _selectedFile;
        set
        {
            _selectedFile = value;
            RaisePropertyChanged();
        }
    }

    public string MenuType
    {
        get => _menuType;
        set => _menuType = value;
    }
    

    #endregion
    
    
    
    #region Initialization
    public EquipmentTreeViewModel(IEventAggregator eventAggregator, EquipmentTreeModel model, NotificationManager notificationManager)
    {
        AddCategoryCommand = new DelegateCommand(async () => await OnCreateFolderAsync());
        EditCommand = new DelegateCommand(OnEdit);
        AddFileCommand = new DelegateCommand(async () => await OnCreateFileAsync());
        OpenFileCommand = new DelegateCommand<object>(OnOpenFile);

        
        
        _eventAggregator = eventAggregator;
        _model = model;
        _notificationManager = notificationManager;
        
        Folders = new ObservableCollection<Folder>();
        Files = new ObservableCollection<File>();
    }
    #endregion
    
    #region LoadTree
   private async Task <ObservableCollection<Folder>> LoadTreeAsync()
    {
    try
    {
        List<EquipmentCategory> categories = await _model.GetCategoriesAsync();
        List<FileEntity> files = await _model.GetFilesAsync();
        
        var folders = categories.Select(c => new Folder
        {
            Id = c.Id,
            FileName = c.CategoryName,
        }).ToList();
        
        var filesDisct = files.GroupBy(f => f.CategoryId).ToDictionary(g => g.Key, g => g.Select(f => new File
        {
            Id = f.Id,
            FileName = f.FileName,
            FolderId = f.CategoryId,
        }).ToList());
        
        var parentIdMap = categories.ToDictionary(c => c.Id, c => c.ParentId);
        var lookup = folders.ToLookup(f => parentIdMap[f.Id]);

        foreach (var folder in folders)
        {
            folder.SubFolders = new ObservableCollection<Folder>(lookup[folder.Id]);

            if (filesDisct.TryGetValue(folder.Id, out var folderFiles))
            {
                folder.Files = new ObservableCollection<File>(folderFiles);
            }
            
        }
        var result = new ObservableCollection<Folder>(folders.Where(f => parentIdMap[f.Id] == null));
        return result;
    }
    
    catch (Exception ex)
    {
        Console.WriteLine(ex);
        return new ObservableCollection<Folder>();
    }
    }
   #endregion

   #region OnCreateFile
   private async Task OnCreateFileAsync()
   {
       if (SelectedFolder != null)
       {
           int categoryId = SelectedFolder.Id;
           string categoryType = MenuType;
           string fileName = SelectedFolder.FileName;

           bool haveChilds = await _model.CheckChildsAsync(categoryId);
           bool IsFinal = await _model.CheckFinal(categoryId);
    
           if (haveChilds)
           {
               _notificationManager.Show("", "Для створення таблиці, будь-ласка, виберіть кінцеву папку", NotificationType.Error);
           }
           else if (IsFinal)
           {
               _notificationManager.Show("", "В цій папці вже створено таблиці", NotificationType.Error);
           }
           else
           {
               await _model.SetFinalAsync(categoryId);
               try
               {
                   var newFileEntity = await _model.CreateNewFileAsync(categoryId, categoryType, fileName);
                   var newFile = new File
                   {
                       Id = newFileEntity.Id,
                       FileName = newFileEntity.FileName,
                       FolderId = SelectedFolder.Id
                   };
            
                   SelectedFolder.AddFile(newFile);
               }
               catch (Exception e)
               {
                   _notificationManager.Show("", $"Ошибка: {e.Message}", NotificationType.Error);
               }
           }
       }
       else
       {
           _notificationManager.Show("", "Для створення таблиці, будь-ласка, виберіть кінцеву папку", NotificationType.Error);
       }
    }
   #endregion
   
    #region OnCreateCategory
    private async Task OnCreateFolderAsync()
    {
        string name = "Нова категорія";
        try
        {
            int? parentId = SelectedFolder?.Id;
            var newCategory = await _model.CreateCategoryAsync(name, parentId);
            
            var newFolder = new Folder
            {
                Id = newCategory.Id,
                FileName = newCategory.CategoryName
            };
            
            if (SelectedFolder != null)
            {
                SelectedFolder.AddFolder(newFolder);
            }
            else
            {
                Folders.Add(newFolder);
            }
            _notificationManager.Show("",$"Створено нову категорію: '{newCategory.CategoryName}'", NotificationType.Information);
        }
        catch (Exception e)
        {
            _notificationManager.Show("", $"Помилка додавання в ObservableCollection: {e.Message}");
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
            _editedName = editedItem.FileName;
        }
    }
    #endregion
    #region ExecuteItemEndEdit
    private async Task ExecuteItemEndEditAsync()
    {
        string newName = SelectedFolder?.FileName;

       if (SelectedFolder == null)
       {
           _notificationManager.Show("", "Виберіть категорію для редагування", NotificationType.Error);
           return;
       }

       if (_editedName == newName)
       {
           _notificationManager.Show("", "Назва не змінилась", NotificationType.Information);
           return;
       }

       try
       {
            var existingCategories = (await _model.GetCategoriesAsync()).FirstOrDefault(c => c.Id == SelectedFolder.Id);
            int? parentId = existingCategories?.ParentId;

            var updatedCategory = await _model.EditCategoryAsync(SelectedFolder.Id, newName, parentId);
            
            SelectedFolder.FileName = updatedCategory.CategoryName;
            
            _notificationManager.Show("", $"Оновлено категорію: '{updatedCategory.CategoryName}'", NotificationType.Information);
       }
       catch (Exception e)
       {
           _notificationManager.Show("", $"Помилка оновлення категорії: {e.Message}");
       }
    }

    #endregion
    #endregion

    #region OnOpenFile
    private void OnOpenFile(object obj)
    {
        if (obj is File file)
        {
            Console.WriteLine($"File: {file.FileName}");
        }
    }
    

    #endregion

    public void OnNavigatedTo(NavigationContext navigationContext)
    {
        if (navigationContext.Parameters.ContainsKey("MenuType"))
        {
            string key = navigationContext.Parameters.GetValue<string>("MenuType");
            _model.SetMenuType(key);
            MenuType = key;
        }

        Task.Run(async () =>
        {
            Folders = await LoadTreeAsync();
        });
    }

    public bool IsNavigationTarget(NavigationContext navigationContext) => true;

    public void OnNavigatedFrom(NavigationContext navigationContext)
    {
    }
}