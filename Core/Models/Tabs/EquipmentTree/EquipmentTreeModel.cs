using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;

using Microsoft.EntityFrameworkCore;

using Core.Services.Log;
using Data.AppDbContext;
using Data.Entities;
using Syncfusion.Data.Extensions;
using Syncfusion.PMML;


namespace Core.Models.Tabs.ProductionEquipmentTree;

public class EquipmentTreeModel
{
    public Folder SelectedFolder { get; set; }

    private readonly AppDbContext _context;
    private LogService _logService;

    private string _originalCategoryName;
    private int? _originalCategoryId;

    private string _currentMenuType;

    private readonly Dictionary<string, IQueryable<EquipmentCategory>> _categorySets;

    public EquipmentTreeModel(AppDbContext context, LogService logService)
    {
        _context = context;
        _logService = logService;

        _categorySets = new Dictionary<string, IQueryable<EquipmentCategory>>
        {
            {"Виробниче обладнання", _context.CategoriesProductionEquipment},
            { "Інструменти", _context.CategoriesTool },
            { "Меблі", _context.CategoriesFurniture },
            { "Офісна техніка", _context.CategoriesOfficeTechnique }
        };
    }
    
    #region SetMenuType
    public void SetMenuType(string menuType) => _currentMenuType = menuType; //Gets menu category
    #endregion

    #region SetCategory
    private IQueryable<EquipmentCategory> GetCategorySet()
    {

        if (!_categorySets.TryGetValue(_currentMenuType, out var set))
        {
            Console.WriteLine($"Ошибка: {_currentMenuType} не найден в _categorySets!");
            throw new ArgumentException($"Невідомий тип меню: {_currentMenuType}");
        }

        if (set == null)
        {
            Console.WriteLine($"Ошибка: Коллекция для {_currentMenuType} равна null!");
            throw new Exception($"Коллекция для {_currentMenuType} равна null!");
        }
        
        return set;
    }
    #endregion

    #region GetCategories from db
    public async Task<List<EquipmentCategory>> GetCategoriesAsync()
    {
        var categorySet = GetCategorySet();
    
        var categories = await categorySet.AsNoTracking().ToListAsync();
    
        return categories;
    }
    #endregion
    
    #region GetFiles from db
    public async Task<List<FileEntity>> GetFilesAsync() =>
    await _context.Files.AsNoTracking().ToListAsync();
    #endregion
    

    public async Task<EquipmentCategory> CreateCategoryAsync(string name, int? parentId = null)
    {
        Console.WriteLine("var categories = await GetCategoriesAsync();");
        var categories = await GetCategoriesAsync();
        Console.WriteLine(categories);
        var uniqueName = GenerateUniqueName(name, categories);
        Console.WriteLine("GenerateUniqueName");

        EquipmentCategory newCategory = _currentMenuType switch
        {
            "Виробниче обладнання" => new CategoryProductionEquipment { CategoryName = uniqueName, ParentId = parentId },
            "Інструменти" => new CategoryTool { CategoryName = uniqueName, ParentId = parentId },
            "Меблі" => new CategoryFurniture { CategoryName = uniqueName, ParentId = parentId },
            "Офісна техніка" => new CategoryOfficeTechnique { CategoryName = uniqueName, ParentId = parentId },
            _ => throw new ArgumentException("Невідомий тип меню")
        };

        try
        {
            await _context.AddAsync(newCategory);
            Console.WriteLine($"Сохранение категории {newCategory.CategoryName}");
            await _context.SaveChangesAsync();
            Console.WriteLine($"Категория {newCategory.CategoryName} сохранена!");

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return newCategory;
    }
    
    #region Deleting (no using now)
    /*public bool Deleting(int categoryId, out string message)
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
    }*/
    #endregion

    #region CheckChilds
    public async Task<bool> CheckChildsAsync (int folderId) =>
        await GetCategorySet().AnyAsync(c => c.ParentId == folderId); 
    #endregion

    public async Task<bool> CheckFinal(int folderId)
    {
        var folder = await GetCategorySet().FirstOrDefaultAsync(c => c.Id == folderId);
        
        return folder.IsFinal;
    }

    #region SetFinal
    public async Task SetFinalAsync(int categoryId)
    {
        var category = await GetCategorySet().FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category != null)
        {
            category.IsFinal = true;
            await _context.SaveChangesAsync();
        }
    }
    #endregion

    #region F2 imitation for editing
    public void F2Imitation() => SendKeys.SendWait("{F2}");
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
    public async Task <EquipmentCategory> EditCategoryAsync (int categoryId, string name, int? parentId = null)
    {
        var category = await GetCategorySet().FirstOrDefaultAsync(c => c.Id == categoryId)
            ?? throw new ArgumentException($"Категорія з ID: {categoryId} не знайдена");
        
        category.CategoryName = GenerateUniqueName(name, await GetCategoriesAsync());
        category.ParentId = parentId;
        await _context.SaveChangesAsync();
        return category;
    }
    #endregion
    #endregion

    #region CreateNewFile

    public async Task <FileEntity> CreateNewFileAsync(int categoryId, string categoryType, string fileName)
    {
        var newFile = new FileEntity
        {
            FileName = fileName,
            CategoryType = categoryType,
            CategoryId = categoryId
        };
        await _context.Files.AddAsync(newFile);
        await _context.SaveChangesAsync();
        return newFile;
    } 
    

    #endregion

    #region GenerateUniqueName
    private string GenerateUniqueName(string name, List<EquipmentCategory> categories)
    {
        var uniqueName = name;
        int counter = 1;
        while (categories.Any(c => c.CategoryName == uniqueName))
        {
            Console.WriteLine($"Проверка имени: {uniqueName}, найдено совпадение, counter = {counter}");
            uniqueName = $"{name} ({counter})";
            counter++;
        }
        Console.WriteLine($"Уникальное имя сгенерировано: {uniqueName}");
        return uniqueName;
    }
    #endregion
}