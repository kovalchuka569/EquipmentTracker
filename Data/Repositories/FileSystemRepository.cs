using Microsoft.EntityFrameworkCore;

using Models.Entities.FileSystem;

using Data.ApplicationDbContext;
using Data.Interfaces;

using Common.Enums;
using Models.Entities.EquipmentSheet;

namespace Data.Repositories;

public class FileSystemRepository(AppDbContext context) : IFileSystemRepository
{
    public async Task<List<FileSystemItemEntity>> GetChildsFileSystemItemsByMenuTypeAsync(MenuType menuType, Guid? parentId, CancellationToken ct)
    {
        return await context.FileSystemItems
            .AsNoTracking()
            .Where(f => f.ParentId == parentId 
                        && f.MenuType == menuType)
            .ToListAsync(ct);
    }

    public async Task AddFileSystemItemAsync(FileSystemItemEntity item, CancellationToken ct)
    {
        await context.FileSystemItems.AddAsync(item, ct);
    }

    public void UpdateFileSystemItemAsync(FileSystemItemEntity item)
    {
        context.FileSystemItems.Update(item);
    }

    public async Task RenameFileSystemItemAsync(Guid fileSystemItemId, string newName, CancellationToken ct)
    {
        await context.FileSystemItems
            .Where(x => x.Id == fileSystemItemId)
            .ExecuteUpdateAsync(f => f
                .SetProperty(i => i.Name, newName), ct);
    }

    public async Task SoftDeleteFileSystemItemAsync(Guid fileSystemItemId, CancellationToken ct)
    {
        await context.FileSystemItems
            .Where(x => x.Id == fileSystemItemId)
            .ExecuteUpdateAsync(f => f
                .SetProperty(i => i.Deleted, true), ct);
    }

    public async Task MoveFileSystemItemAsync(Guid fileSystemItemId, Guid newParentId, CancellationToken ct)
    {
        await context.FileSystemItems
            .Where(x => x.Id == fileSystemItemId)
            .ExecuteUpdateAsync(f => f
                .SetProperty(i => i.ParentId, newParentId), ct);
    }

    public async Task UpdateHasChildsAsync(Guid fileSystemItemId, bool hasChilds, CancellationToken ct)
    {
        await context.FileSystemItems
            .Where(x => x.Id == fileSystemItemId)
            .ExecuteUpdateAsync(f => f
                .SetProperty(i => i.HasChilds, hasChilds), ct);
    }

    public async Task UpdateHasChildsAsync(List<(Guid Id, bool HasChilds)> newStatuses, CancellationToken ct)
    {
        
        var ids = newStatuses.Select(x => x.Id).ToList();
        
        var entities = await context.FileSystemItems
            .Where(x => ids.Contains(x.Id))
            .ToListAsync(ct);

        foreach (var entity in entities)
        {
            var updated = newStatuses.First(x => x.Id == entity.Id);
            entity.HasChilds = updated.HasChilds;
        }
    }

    public async Task UpdateParentsAndOrdersAsync(List<(Guid Id, Guid? NewParentId, int NewOrder)> newPositions, CancellationToken ct)
    {
        var ids = newPositions.Select(x => x.Id).ToList();
        
        var entities = await context.FileSystemItems
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(ct);
        
        foreach (var entity in entities)
        {
            var updated = newPositions.First(x => x.Id == entity.Id);
            entity.Order = updated.NewOrder;
            entity.ParentId = updated.NewParentId;
        }
    }
}