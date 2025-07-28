using Data.ApplicationDbContext;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Entities.FileSystem;
using Models.Enums;

namespace Data.Repositories;

public class FoldersRepository(AppDbContext context) : IFoldersRepository
{

    public async Task<FolderEntity> GetByIdAsync(Guid id, CancellationToken ct)
    { 
        return await context.Folders
                   .AsNoTracking()
                   .FirstOrDefaultAsync(f => f.Id == id && f.Deleted == false, cancellationToken: ct) 
               ?? throw new InvalidOperationException("Folder not found");
    }

    public async Task<List<FolderEntity>> GetListByMenuTypeAsync(MenuType menuType, CancellationToken ct)
    {
        return await context.Folders
            .AsNoTracking()
            .Where(f => f.MenuType == menuType && f.Deleted == false)
            .ToListAsync(cancellationToken: ct);
    }
    
    public async Task AddAsync(FolderEntity folder, CancellationToken ct)
    {
        await context.Folders
            .AddAsync(folder, ct);
    }

    public async Task UpdateAsync(FolderEntity folder, CancellationToken ct)
    {
        await context.Folders
            .ExecuteUpdateAsync(c => c
                .SetProperty(f => f.Name , folder.Name), cancellationToken: ct);
    }

    public async Task RenameAsync(Guid id, string newName, CancellationToken ct = default)
    {
        await context.Folders
            .Where(f => f.Id == id)
            .ExecuteUpdateAsync(c => c
                .SetProperty(f => f.Name, newName), cancellationToken: ct);
    }
    
    public async Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct)
    {
        await context.Folders
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s.
                SetProperty(c => c.Deleted, deleted), cancellationToken: ct);
    }
}