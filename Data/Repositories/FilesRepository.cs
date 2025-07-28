using Data.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Models.Entities.FileSystem;
using Data.Repositories.Interfaces;
using Models.Enums;
using Syncfusion.Data.Extensions;

namespace Data.Repositories;

public class FilesRepository(AppDbContext context) : IFilesRepository
{
    public async Task<FileEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Files
                   .AsNoTracking()
                   .FirstOrDefaultAsync(f => f.Id == id && f.Deleted == false, cancellationToken: ct) 
               ?? throw new InvalidOperationException("File not found");
    }
    
    public async Task<List<FileEntity>> GetListByMenuTypeAsync(MenuType menuType, CancellationToken ct)
    {
        return await context.Files
            .AsNoTracking()
            .Where(f => f.MenuType == menuType && f.Deleted == false)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task AddAsync(FileEntity file, CancellationToken ct)
    {
        await context.Files
            .AddAsync(file, ct);
       
    }

    public async Task UpdateAsync(FileEntity file, CancellationToken ct)
    {
        await context.Files
            .ExecuteUpdateAsync(c => c
                .SetProperty(f => f.Name, file.Name), cancellationToken: ct);
    }

    public async Task RenameAsync(Guid id, string newName, CancellationToken ct = default)
    {
        await context.Files
            .Where(f => f.Id == id)
            .ExecuteUpdateAsync(c => c
                .SetProperty(f => f.Name, newName), cancellationToken: ct);
    }

    public async Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct)
    {
        await context.Files
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s.
                SetProperty(c => c.Deleted, deleted), cancellationToken: ct);
    }
}