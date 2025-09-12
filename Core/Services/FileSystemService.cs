using Common.Enums;
using Common.Logging;
using Models.FileSystem;

using Data.UnitOfWork;

using Core.Interfaces;
using Core.Mappers;
using Core.Services.Base;
using Models.Entities.EquipmentSheet;
using Models.Entities.PivotSheet;

namespace Core.Services;

public class FileSystemService(IUnitOfWork unitOfWork, IAppLogger<FileSystemService> logger) 
    : DatabaseService<FileSystemService>(unitOfWork, logger), IFileSystemService
{
    
    #region Interface realization
    
    public async Task<List<FileSystemItemModel>> GetChildsAsync(MenuType menuType, Guid? parentId, CancellationToken ct)
    {
        return await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.EnsureInitializedForReadAsync(ct);
        
            var entities = await UnitOfWork.FileSystemRepository
                .GetChildsFileSystemItemsByMenuTypeAsync(menuType, parentId, ct);

            return entities.Select(FileSystemMapper.FileSystemItemEntityToModel)
                .ToList();
            
        }, nameof(GetChildsAsync), ct);
    }

    public async Task InsertChildAsync(FileSystemItemModel fileSystemItemModel, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                
                switch (fileSystemItemModel)
                {
                    case EquipmentSheetFileModel { EquipmentSheetId: not null } efModel:
                    {
                        var sheetEntity = new EquipmentSheetEntity
                        {
                            Id = efModel.EquipmentSheetId.Value,
                        };
                        await UnitOfWork.EquipmentSheetRepository.AddAsync(sheetEntity, ct);
                        break;
                    }
                    case PivotSheetFileModel { PivotSheetId: not null } pfModel:
                    {
                        var pivotEntity = new PivotSheetEntity
                        {
                            Id = pfModel.PivotSheetId.Value,
                        };
                        await UnitOfWork.PivotSheetRepository.AddAsync(pivotEntity, ct);
                        break;
                    }
                }
                await UnitOfWork.FileSystemRepository.AddFileSystemItemAsync(FileSystemMapper.FileSystemItemModelToEntity(fileSystemItemModel), ct);
                
            }, ct);
            
        }, nameof(InsertChildAsync), ct);
    }

    public async Task DeleteChildAsync(FileSystemItemModel fileSystemItemModel, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(() => throw new NotImplementedException(), nameof(DeleteChildAsync), ct);
    }

    public async Task RenameFileSystemItemAsync(Guid renamedFileId, string newName, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.FileSystemRepository.RenameFileSystemItemAsync(renamedFileId, newName, ct);
                
            }, ct);
            
        }, nameof(RenameFileSystemItemAsync), ct);
    }

    public async Task UpdateHasChildsAsync(Guid fileSystemItemId, bool hasChilds, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.FileSystemRepository.UpdateHasChildsAsync(fileSystemItemId, hasChilds, ct);
                
            }, ct);
            
        }, nameof(UpdateHasChildsAsync), ct);
    }

    public async Task UpdateHasChildsAsync(List<(Guid Id, bool HasChilds)> newStatuses, CancellationToken ct = default)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.FileSystemRepository.UpdateHasChildsAsync(newStatuses, ct);
                
            }, ct);
            
        }, nameof(UpdateHasChildsAsync), ct);
    }


    public async Task UpdateParentsAndOrdersAsync(List<(Guid Id, Guid? NewParentId, int NewOrder)> newPositions, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            await UnitOfWork.ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.FileSystemRepository.UpdateParentsAndOrdersAsync(newPositions, ct);
                
            }, ct);
            
        }, nameof(UpdateParentsAndOrdersAsync), ct);
    }
    
    #endregion
}