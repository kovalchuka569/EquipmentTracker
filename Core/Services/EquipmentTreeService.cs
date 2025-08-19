using System.Collections.ObjectModel;

using Core.Interfaces;

using Models.Entities.EquipmentSheet;
using Models.Entities.FileSystem;
using Models.Entities.SummarySheet;
using Models.EquipmentTree;
using Models.Enums;

using Data.UnitOfWork;

namespace Core.Services;

public class EquipmentTreeService(IUnitOfWork unitOfWork)
    : IEquipmentTreeService
{                   
    public async Task<ObservableCollection<IFileSystemItem>> BuildHierarchy(MenuType menuType)
    {
        await unitOfWork.EnsureInitializedForReadAsync();
        
        var foldersFromDb = await unitOfWork.FoldersRepository.GetListByMenuTypeAsync(menuType);
        var filesFromDb = await unitOfWork.FilesRepository.GetListByMenuTypeAsync(menuType);

        var folderItems = foldersFromDb.Select(f => new FolderItem
        {
            Id = f.Id,
            Name = f.Name,
            FolderId = f.FolderId
        }).ToList();

        var fileItems = filesFromDb.Select(f => new FileItem
        {
            Id = f.Id,
            Name = f.Name,
            FolderId = f.FolderId,
            FileFormat = f.FileFormat,
            EquipmentSheetId = (f as EquipmentFileEntity)?.EquipmentSheetId ?? Guid.Empty,
            SummaryId = (f as SummaryFileEntity)?.SummarySheetId ?? Guid.Empty,
        }).ToList();
        
        
        var folderDict = folderItems.ToDictionary(f => f.Id);
        
        // Adding files in folders
        foreach (var file in fileItems)
        {
            if (file.FolderId.HasValue && folderDict.TryGetValue(file.FolderId.Value, out var parentFolder))
            {
                parentFolder.AddFile(file);
            }
        }
        
        // Adding SubFolders
        foreach (var folder in folderItems)
        {
            if (folder.FolderId is not null && folderDict.TryGetValue(folder.FolderId.Value, out var parentFolder))
            {
                parentFolder.AddFolder(folder);
            }
        }

        return new ObservableCollection<IFileSystemItem>(folderItems
            .Where(f => f.FolderId is null));
    }

    public async Task CreateFolderAsync(FolderItem folderItem, CancellationToken ct)
    {
        var fileEntity = new FolderEntity
        {
            Id = folderItem.Id,
            Name = folderItem.Name,
            FolderId = folderItem.FolderId,
            MenuType = folderItem.MenuType,
        };
        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.FoldersRepository.AddAsync(fileEntity, ct);
        await unitOfWork.CommitAsync(ct);
    }

    public async Task CreateFileAsync(FileItem fileItem, FileFormat fileFormat, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);
        switch (fileFormat)
        {
            case FileFormat.EquipmentSheet:
                var newEquipmentSheet = new EquipmentSheetEntity
                {
                    Id = fileItem.EquipmentSheetId,
                    Deleted = false
                };
                await unitOfWork.EquipmentSheetRepository.AddAsync(newEquipmentSheet, ct);

                var newEquipmentFileEntity = new EquipmentFileEntity
                {
                    Id = fileItem.Id,
                    FolderId = fileItem.FolderId,
                    Name = fileItem.Name,
                    FileFormat = fileFormat,
                    MenuType = fileItem.MenuType,
                    EquipmentSheetId = fileItem.EquipmentSheetId,
                };
                await unitOfWork.FilesRepository.AddAsync(newEquipmentFileEntity, ct);
                break;
            
            case FileFormat.SummaryEquipment:
                var newSummarySheetEntity = new SummarySheetEntity
                {
                    Id = fileItem.SummaryId,
                    Deleted = false
                };
                await unitOfWork.SummarySheetRepository.AddAsync(newSummarySheetEntity, ct);

                var newSummaryFileEntity = new SummaryFileEntity
                {
                    Id = fileItem.Id,
                    FolderId = fileItem.FolderId,
                    Name = fileItem.Name,
                    FileFormat = fileFormat,
                    MenuType = fileItem.MenuType,
                    SummarySheetId = fileItem.SummaryId
                };
                await unitOfWork.FilesRepository.AddAsync(newSummaryFileEntity, ct);
                break;
            default:
                    var newDefaultFileEntity = new FileEntity
                    {
                        Id = fileItem.Id,
                        FolderId = fileItem.FolderId,
                        Name = fileItem.Name,
                        FileFormat = fileFormat,
                        MenuType = fileItem.MenuType,
                    };
                    await unitOfWork.FilesRepository.AddAsync(newDefaultFileEntity, ct);
                break;
        }
        await unitOfWork.CommitAsync(ct);
    }


    public string GenerateUniqueFileFolderName(string baseName, List<string> existingNames)
    {
        if (!existingNames.Contains(baseName)) return baseName;

        int counter = 1;
        string newName;
        do
        {
            newName = $"{baseName} ({counter})";
            counter++;
        } while (existingNames.Contains(newName));
        return newName;
    }

    public async Task RenameFolderAsync(Guid folderId, string newName, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.FoldersRepository.RenameAsync(folderId, newName, ct);
        await unitOfWork.CommitAsync(ct);
    }

    public async Task RenameFileAsync(Guid fileId, string newName, CancellationToken ct)
    {
        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.FilesRepository.RenameAsync(fileId, newName, ct);
        await unitOfWork.CommitAsync(ct);
    }
}