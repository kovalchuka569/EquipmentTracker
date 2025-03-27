using System.Collections.ObjectModel;
using System.Windows.Forms;

using Microsoft.EntityFrameworkCore;

using Core.Services.Log;
using Data.AppDbContext;
using Data.Entities;
using Syncfusion.Data.Extensions;


namespace Core.Models.Tabs.ProductionEquipmentTree;

public class EquipmentTreeModel
{
    public Folder SelectedFolder { get; set; }

    private readonly AppDbContext _context;
    private LogService _logService;

    private string _originalCategoryName;
    private int? _originalCategoryId;

    private string _currentMenuType;

    public void SetMenuType(string menuType)
    {
        _currentMenuType = menuType;
    }

    public EquipmentTreeModel(AppDbContext context, LogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public List<EquipmentCategory> GetCategories()
    { 
        return _currentMenuType switch
        {
            "Виробниче обладнання" => _context.CategoriesProductionEquipment.AsNoTracking().ToList<EquipmentCategory>(),
            "Інструменти" => _context.CategoriesTool.AsNoTracking().ToList<EquipmentCategory>(),
            "Меблі" => _context.CategoriesFurniture.AsNoTracking().ToList<EquipmentCategory>(),
            "Офісна техніка" => _context.CategoriesOfficeTechnique.AsNoTracking().ToList<EquipmentCategory>(),
            _ => new List<EquipmentCategory>()
        };
    }

    public List<FileEntity> GetFiles()
    {
        return _context.Files.AsNoTracking().ToList();
    }

    public EquipmentCategory CreateCategory(string name, int? parentId = null)
    {
        //Get existing categories for make unique name
        List<EquipmentCategory> existingCategories = GetCategories();
        
        string uniqueName = name;
        int counter = 1;
        //If the names are the same, we make them unique
        while (existingCategories.Any(c => c.CategoryName == uniqueName))
        {
            uniqueName = $"{name} ({counter})";
            counter++;
        }
        
        //Creating new object
        EquipmentCategory newCategory = _currentMenuType switch
        {
            "Виробниче обладнання" => new CategoryProductionEquipment { CategoryName = uniqueName, ParentId = parentId },
            "Інструменти" => new CategoryTool() { CategoryName = uniqueName, ParentId = parentId },
            "Меблі" => new CategoryFurniture() { CategoryName = uniqueName, ParentId = parentId },
            "Офісна техніка" => new CategoryOfficeTechnique() { CategoryName = uniqueName, ParentId = parentId },
            _ => throw new ArgumentException($"Невідомий тип меню: {_currentMenuType}")
        };
        
        //Add new object to database
        switch (_currentMenuType)
        {
            case "Виробниче обладнання":
                _context.CategoriesProductionEquipment.Add((CategoryProductionEquipment)newCategory);
                break;
            case "Інструменти":
                _context.CategoriesTool.Add((CategoryTool)newCategory);
                break;
            case "Меблі":
                _context.CategoriesFurniture.Add((CategoryFurniture)newCategory);
                break;
            case "Офісна техніка":
                _context.CategoriesOfficeTechnique.Add((CategoryOfficeTechnique)newCategory);
                break;
        }
        _context.SaveChanges();
        
        //Return new object for update Observable Collection (UI) in ViewModel
        return newCategory;
    }
    
    #region Deleting 
    public bool Deleting(int categoryId, out string message)
    {
        try
        {
        object categoryToDelete = null;
        
        switch (_currentMenuType)
        {
            case "Виробниче обладнання":
                categoryToDelete = _context.CategoriesProductionEquipment
                    .FirstOrDefault(c => c.Id == categoryId)!;
                break;

            case "Меблі":
                categoryToDelete = _context.CategoriesFurniture
                    .FirstOrDefault(c => c.Id == categoryId)!;
                break;

            case "Інструменти":
                categoryToDelete = _context.CategoriesTool
                    .FirstOrDefault(c => c.Id == categoryId)!;
                break;

            case "Офісна техніка":
                categoryToDelete = _context.CategoriesOfficeTechnique
                    .FirstOrDefault(c => c.Id == categoryId)!;
                break;
        }
        
        if (categoryToDelete == null)
        {
            message = "Категорію не знайдено.";
            return false;
        }
        
        var hasChildren = false;
        switch (_currentMenuType)
        {
            case "Виробниче обладнання":
                hasChildren = _context.CategoriesProductionEquipment
                    .Any(c => c.ParentId == categoryId);
                break;

            case "Меблі":
                hasChildren = _context.CategoriesFurniture
                    .Any(c => c.ParentId == categoryId);
                break;

            case "Інструменти":
                hasChildren = _context.CategoriesTool
                    .Any(c => c.ParentId == categoryId);
                break;

            case "Офісна техніка":
                hasChildren = _context.CategoriesOfficeTechnique
                    .Any(c => c.ParentId == categoryId);
                break;
        }
        
        if (hasChildren)
        {
            message = "Неможливо видалити категорію, оскільки вона містить записи";
            return false;
        }
        
        switch (_currentMenuType)
        {
            case "Виробниче обладнання":
                _context.CategoriesProductionEquipment.Remove((CategoryProductionEquipment)categoryToDelete);
                break;

            case "Меблі":
                _context.CategoriesFurniture.Remove((CategoryFurniture)categoryToDelete);
                break;

            case "Інструменти":
                _context.CategoriesTool.Remove((CategoryTool)categoryToDelete);
                break;

            case "Офісна техніка":
                _context.CategoriesOfficeTechnique.Remove((CategoryOfficeTechnique)categoryToDelete);
                break;
        }
            _context.SaveChanges();
            _logService.AddLog($"Видалено категорію: {((dynamic)categoryToDelete).CategoryName}, Id: {((dynamic)categoryToDelete).Id}");
            message = $"Категорія '{((dynamic)categoryToDelete).CategoryName}' успішно видалена.";
            SelectedFolder = null;
            return true;
        }
        catch (Exception e)
        {
            message = $"Помилка видалення: {e.Message}";
            return false;
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
    public void BeginEditing(Folder editedItem)
    {
        if (editedItem != null)
        {
            _originalCategoryName = editedItem.FileName;
            _originalCategoryId = editedItem.Id;
        }
    }
    #endregion
    #region End editing
    public EquipmentCategory EditCategory (int categoryId, string name, int? parentId = null)
    {

        EquipmentCategory categoryToEdit = null;

        switch (_currentMenuType)
        {
            case "Виробниче обладнання":
                categoryToEdit = _context.CategoriesProductionEquipment.FirstOrDefault(c => c.Id == categoryId)!;
                break;
            case "Інструменти":
                categoryToEdit = _context.CategoriesTool.FirstOrDefault(c => c.Id == categoryId)!;
                break;
            case "Меблі":
                categoryToEdit = _context.CategoriesFurniture.FirstOrDefault(c => c.Id == categoryId)!;
                break;
            case "Офісна техніка":
                categoryToEdit = _context.CategoriesOfficeTechnique.FirstOrDefault(c => c.Id == categoryId)!;
                break;
            default:
                throw new ArgumentException($"Невідомий тип меню: {_currentMenuType}");
        }

        if (categoryToEdit == null)
        {
            throw new ArgumentException($"Категорія з Id: {categoryId} не знайдена");
        }
        
        //Get existing categories for make unique name
        List<EquipmentCategory> existingCategories = GetCategories();
        
        string uniqueName = name;
        int counter = 1;
        //If the names are the same, we make them unique
        while (existingCategories.Any(c => c.CategoryName == uniqueName))
        {
            uniqueName = $"{name} ({counter})";
            counter++;
        }
        categoryToEdit.CategoryName = uniqueName;
        categoryToEdit.ParentId = parentId;
        
        _context.SaveChanges();
        return categoryToEdit;
    }
    #endregion
    #endregion

    #region Adding file

    public FileEntity CreateNewFile(int categoryId, string categoryType, string fileName)
    {
        var newFile = new FileEntity
        {
            FileName = fileName,
            CategoryType = categoryType,
            CategoryId = categoryId
        };
        _context.Files.Add(newFile);
        _context.SaveChanges();
        return newFile;
    } 
    

    #endregion
}