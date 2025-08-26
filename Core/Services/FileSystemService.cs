using Common.Enums;

using Models.FileSystem;

using Data.UnitOfWork;

using Core.Interfaces;
using Core.Mappers;
using Models.Entities.EquipmentSheet;
using Models.Entities.PivotSheet;

namespace Core.Services;

public class FileSystemService(IUnitOfWork unitOfWork) : IFileSystemService
{
    public async Task<List<FileSystemItemModel>> GetChildsAsync(MenuType menuType, Guid? parentId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        
        var entities = await unitOfWork.FileSystemRepository
            .GetChildsFileSystemItemsByMenuTypeAsync(menuType, parentId, ct);

        return entities.Select(FileSystemMapper.FileSystemItemEntityToModel)
            .ToList();
    }

    public async Task InsertChildAsync(FileSystemItemModel fileSystemItemModel, CancellationToken ct)
    {
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            switch (fileSystemItemModel)
            {
                case EquipmentSheetFileModel { EquipmentSheetId: not null } efModel:
                {
                    var sheetEntity = new EquipmentSheetEntity
                    {
                        Id = efModel.EquipmentSheetId.Value,
                    };
                    await unitOfWork.EquipmentSheetRepository.AddAsync(sheetEntity, ct);
                    break;
                }
                case PivotSheetFileModel { PivotSheetId: not null } pfModel:
                {
                    var pivotEntity = new PivotSheetEntity
                    {
                        Id = pfModel.PivotSheetId.Value,
                    };
                    await unitOfWork.PivotSheetRepository.AddAsync(pivotEntity, ct);
                    break;
                }
            }

            await unitOfWork.FileSystemRepository.AddFileSystemItemAsync(FileSystemMapper.FileSystemItemModelToEntity(fileSystemItemModel), ct);
        }, ct);
    }

    public Task DeleteChildAsync(FileSystemItemModel fileSystemItemModel, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task RenameFileSystemItemAsync(Guid renamedFileId, string newName, CancellationToken ct)
    {
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.FileSystemRepository.RenameFileSystemItemAsync(renamedFileId, newName, ct);
        }, ct);
    }

    public async Task UpdateHasChildsAsync(Guid fileSystemItemId, bool hasChilds, CancellationToken ct)
    {
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.FileSystemRepository.UpdateHasChildsAsync(fileSystemItemId, hasChilds, ct);
        }, ct);
    }

    public async Task UpdateHasChildsAsync(List<(Guid Id, bool HasChilds)> newStatuses, CancellationToken ct = default)
    {
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.FileSystemRepository.UpdateHasChildsAsync(newStatuses, ct);
        }, ct);
    }


    public async Task UpdateParentsAndOrdersAsync(List<(Guid Id, Guid? NewParentId, int NewOrder)> newPositions, CancellationToken ct)
    {
        await unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.FileSystemRepository.UpdateParentsAndOrdersAsync(newPositions, ct);
        }, ct);
    }
}