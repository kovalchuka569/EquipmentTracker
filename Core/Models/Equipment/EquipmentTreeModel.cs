using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Forms;
using Core.Models.Tabs.ProductionEquipmentTree;
using Microsoft.EntityFrameworkCore;

using Core.Services.Log;
using Data.AppDbContext;
using Data.Entities;
using Syncfusion.Data.Extensions;
using Syncfusion.PMML;
using DbContext = Data.AppDbContext.DbContext;


namespace Core.Models.EquipmentTree;

public class EquipmentTreeModel
{

    private readonly DbContext _context;
    private LogService _logService;
    

    private string _currentMenuType;
    

    private readonly Dictionary<string, IQueryable<EquipmentCategory>> _categorySets;

    public EquipmentTreeModel(DbContext context, LogService logService)
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
    
    #region CheckChilds
    public async Task<bool> CheckChildsAsync (int folderId) =>
        await GetCategorySet().AnyAsync(c => c.ParentId == folderId); 
    #endregion

    public async Task<bool> CheckFinal(int folderId)
    {
        var folder = await GetCategorySet().FirstOrDefaultAsync(c => c.Id == folderId);
        return folder != null && folder.IsFinal;
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