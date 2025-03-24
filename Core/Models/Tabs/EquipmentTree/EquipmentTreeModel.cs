

using System.Collections.ObjectModel;
using System.Windows.Forms;
using Core.Services.Log;
using Core.Services.Notifications;
using Microsoft.EntityFrameworkCore;
using Data.AppDbContext;
using Data.Entities;
using Syncfusion.UI.Xaml.TreeView;
using Syncfusion.UI.Xaml.TreeView.Engine;

using Ookii.Dialogs.Wpf;
using TaskDialog = Ookii.Dialogs.Wpf.TaskDialog;
using TaskDialogButton = Ookii.Dialogs.Wpf.TaskDialogButton;

namespace Core.Models.Tabs.ProductionEquipmentTree;

public class EquipmentTreeModel
{
    public ObservableCollection<Folder> Folders { get; set; }
    public Folder SelectedFolder { get; set; }

    private readonly AppDbContext _context;
    private SfTreeView _treeView;
    private LogService _logService;

    private string _originalCategoryName;
    private int? _originalCategoryId;

    private readonly BusyIndicatorService _busyIndicatorService;

    public EquipmentTreeModel(AppDbContext context, BusyIndicatorService busyIndicatorService, LogService logService)
    {
        _context = context;
        _busyIndicatorService = busyIndicatorService;
        _logService = logService;
    }

    public void LoadData()
    {
        var expandedNodes = SaveExpandedNodes();
        var categories = _context.CategoriesProductionEquipment
            .AsNoTracking()
            .ToList();

        Folders = new ObservableCollection<Folder>();

        Folders = new ObservableCollection<Folder>(BuildFolderHierarchy(categories, null));

        RestoreExpandedNodes(expandedNodes);
    }

    #region Folder hierarchy builder

    private IEnumerable<Folder> BuildFolderHierarchy(List<CategoryProductionEquipment> categories, int? parentId)
    {
        return categories
            .Where(c => c.ParentId == parentId)
            .OrderBy(c => c.CategoryName,
                Comparer<string>.Create((a, b) =>
                    string.Compare(a, b, StringComparison.CurrentCultureIgnoreCase))) //Sorting
            .Select(c => new Folder
            {
                Id = c.Id,
                FileName = c.CategoryName,
                SubFolders = BuildFolderHierarchy(categories, c.Id).ToList()
            });
    }

    #endregion

    #region Managing node states

    //Save expanded nodes
    private List<string> SaveExpandedNodes()
    {
        var expanded = new List<string>();
        if (_treeView != null)
        {
            foreach (var node in _treeView.Nodes)
            {
                CollectExpandedNodes(node, expanded);
            }
        }

        return expanded;
    }

    //Collect expanded nodes
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

    //Restore expanded nodes
    private void RestoreExpandedNodes(List<string> expandedNodes)
    {
        if (_treeView != null)
        {
            foreach (var node in _treeView.Nodes)
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
    private string GetUniqueCategoryNameCreatings(string baseName, int? parentId = null)
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
        
        while (_context.CategoriesProductionEquipment.Any(c =>
                   c.CategoryName == newName && c.CategoryName != originalName && c.ParentId == parentId))
        {
            newName = $"{baseName} ({counter})";
            counter++;
        }

        return newName;
    }

    #endregion

    #region Creating category
    public bool Creating(out string message)
    {
        try
        {
            var uniqueCategoryName = GetUniqueCategoryNameCreatings("Нова категорія");
            var newCategory = new CategoryProductionEquipment()
            {
                CategoryName = uniqueCategoryName,
                ParentId = null
            };
            _context.CategoriesProductionEquipment.Add(newCategory);
            _context.SaveChanges();
            _logService.AddLog($"Створено нову категорію: {uniqueCategoryName}");
            message = $"Створено категорію: {uniqueCategoryName}";
            LoadData();
            SelectedFolder = null;
            return true;
        }
        catch (Exception e)
        {
            message = $"Помилка створення: {e.Message}";
            return false;
        }
    }
    #endregion
    #region Creating subcategory
    public bool CreatingSubCategory(object parameter, out string message)
    {
        try
        {
            var parentFolder = parameter as Folder ?? SelectedFolder;

            var parentCategory = _context.CategoriesProductionEquipment.FirstOrDefault(c => c.Id == parentFolder.Id);
            if (parentCategory == null)
            {
                message = $"Батьківська категорія '{parentFolder.FileName}' не знайдена.";
                return false;
            }

            var uniequeCategoryName = GetUniqueCategoryNameCreatings("Нова підкатегорія", parentCategory.Id);
            var newCategory = new CategoryProductionEquipment
            {
                CategoryName = uniequeCategoryName,
                ParentId = parentCategory.Id
            };
            _context.CategoriesProductionEquipment.Add(newCategory);
            _context.SaveChanges();
            _logService.AddLog($"Створено підкатегорію: '{newCategory.CategoryName}'");
            message = $"Створено нову підкатегорію: '{newCategory.CategoryName}'";
            return true;
        }
        catch (Exception e)
        {
            message = $"Помилка створення підкатегорії: {e.Message}";
            return false;
        }
    }


    #endregion

    #region On Delete

    public bool Deleting(int categoryId, out string message)
    {
        try
        {
            var categoryToDelete = _context.CategoriesProductionEquipment
                .FirstOrDefault(c => c.Id == categoryId);

            DeleteCategroyAndChildren(categoryToDelete.Id);
            
            if (categoryToDelete == null)
            {
                message = "Категорію не знайдено.";
                return false;
            }

            _context.CategoriesProductionEquipment.Remove(categoryToDelete);
            _context.SaveChanges();
            _logService.AddLog($"Видалено категорію {categoryToDelete.CategoryName}, Id: {categoryToDelete.Id}");
            message = $"Категорія '{categoryToDelete.CategoryName}' успішно видалена.";
            LoadData();
            return true;
        }
        catch (Exception e)
        {
            message = $"Помилка видалення: {e.Message}";
            return false;
        }
    }
    
    private void DeleteCategroyAndChildren(int categoryId)
    {
        var children = _context.CategoriesProductionEquipment
            .Where(c => c.ParentId == categoryId)
            .ToList();

        foreach (var child in children)
        {
            DeleteCategroyAndChildren(child.Id);
            _context.CategoriesProductionEquipment.Remove(child);
            _context.SaveChanges();
            _logService.AddLog($"Видалено підкатегорію {child.CategoryName}, Id: {child.Id}");
        }
    }
    #endregion
    
    #region F2 imitation for editing
    public void F2Imitation()
    {
            SendKeys.SendWait("{F2}");
    }
    #endregion
    #region Editing tree items
    #region Begin editing
    public bool BeginEditing(Folder editedItem)
    {
        if (editedItem != null)
        {
            _originalCategoryName = editedItem.FileName;
            _originalCategoryId = editedItem.Id;
            return true;
        }
        return false;
    }
    #endregion
    #region ExecuteItemEndEdit
    public bool EndEditing(Folder editedItem, out string message)
    {
        try
        {
            if (editedItem == null)
            {
                message = "Помилка: не вибрано об'єкт для редагування";
                return false;
            }

            var categoryToUpdate =
                _context.CategoriesProductionEquipment.FirstOrDefault(c => c.Id == _originalCategoryId);
            if (categoryToUpdate == null)
            {
                message = "Помилка: не знайдено категорію";
                return false;
            }

            bool isSubCategory = categoryToUpdate.ParentId != null;

            if (editedItem.FileName != _originalCategoryName)
            {
                string oldName = _originalCategoryName;
                string newName = editedItem.FileName;
                
                var uniqueCategoryName = GetUniqueCategoryName(newName, _originalCategoryName,
                    categoryToUpdate.ParentId);
                categoryToUpdate.CategoryName = uniqueCategoryName;
                editedItem.FileName = uniqueCategoryName;
                
                _context.SaveChanges();
                if (isSubCategory)
                {
                    message = $"Підкатегорію перейменовано: '{oldName}' \u2794 '{uniqueCategoryName}'";
                }
                else
                {
                    message = $"Категорію перейменовано: '{oldName}' \u2794 '{uniqueCategoryName}'";
                }
                _logService.AddLog(message);
                return true;
            }
            message = "Назва не змінилась";
            return false;
        }
        catch (Exception e)
        {
            message = $"Помилка: {e.Message}";
            return false;
        }
        finally
        {
            _originalCategoryName = null;
            _originalCategoryId = null;
        }
    }
    #endregion
    #endregion
}