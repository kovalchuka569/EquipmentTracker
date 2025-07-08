using System.Collections.ObjectModel;
using Common.Logging;
using Core.Models.Tabs.EquipmentTree;
using Data.Repositories.EquipmentTree;
using Microsoft.EntityFrameworkCore;
using Models.Equipment;
using Models.EquipmentTree;
using IFileSystem = Core.Models.Tabs.EquipmentTree.IFileSystem;

namespace Core.Services.EquipmentTree;

public class EquipmentTreeService : IEquipmentTreeService
{
    private IAppLogger<EquipmentTreeService> _logger;
    private IEquipmentTreeRepository _repository;
    
    public EquipmentTreeService(IAppLogger<EquipmentTreeService> logger, IEquipmentTreeRepository repository )
    {
        _logger = logger;
        _repository = repository;
    }

    public async Task <ObservableCollection<IFileSystemItem>> BuildHierarchy(string menuType)
    {
        var foldersFromDb = await _repository.GetFoldersAsync(menuType);
        var filesFromDb = await _repository.GetFilesAsync(menuType);

        var folderItems = foldersFromDb.Select(f => new FolderItem
        {
            Id = f.Id,
            Name = f.Name,
            ParentId = f.ParentId
        }).ToList();

        var fileItems = filesFromDb.Select(f => new FileItem
        {
            Id = f.Id,
            Name = f.Name,
            ParentIdFolder = f.FolderId,
            TableName = f.TableName,
            FileType = f.FileType,
            TableId = f.TableId
        }).ToList();
        
        var folderDict = folderItems.ToDictionary(f => f.Id);
        
        // Adding files in folders
        foreach (var file in fileItems)
        {
            if (folderDict.TryGetValue(file.ParentIdFolder, out var parentFolder))
            {
                parentFolder.AddFile(file);
            }
        }
        
        // Adding SubFolders
        foreach (var folder in folderItems)
        {
            if (folder.ParentId != 0 && folderDict.TryGetValue(folder.ParentId.Value, out var parentFolder))
            {
                parentFolder.AddFolder(folder);
            }
        }

        return new ObservableCollection<IFileSystemItem>(folderItems
            .Where(f => f.ParentId == 0));
    }

    public async Task<int> InsertFolderAsync(string name, int? parentId, string menuType)
    {
        try
        {
           return await _repository.InsertFolderAsync(name, parentId, menuType);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error building hierachy");
            throw;
        }
    }

    public async Task<int> InsertFileAsync(string name, int folderId, string menuType)
    {
        try
        {
            return await _repository.InsertFileAsync(name, folderId, menuType);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error inserting folder");
            throw;
        }
    }

    public async Task<string> GenerateUniqueFileFolderNameAsync(string baseName, List<string> existingNames)
    {
        if(!existingNames.Contains(baseName)) return baseName;
        
        int counter = 1;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName));
        return newName;
    }



    public async Task RenameFolderAsync(int folderId, string newName)
    {
        try
        {
            await _repository.RenameFolderAsync(folderId, newName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error renaming folder");
            throw;
        }
    }

    public async Task RenameChildsAsync(int folderId, string newName, string oldName, string menuType)
    {
        try
        {
            await _repository.RenameChildsAsync(folderId, newName, oldName, menuType);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "System error renaming childs");
            throw;
        }
    }
}