using Data.ApplicationDbContext;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Entities.Table;

namespace Data.Repositories;

public class CellsRepository(AppDbContext context) : ICellsRepository
{
    public async Task<CellEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Cells
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct) 
               ?? throw new InvalidOperationException("Cell not found");
    }
    
    public async Task AddAsync(CellEntity cell, CancellationToken ct)
    {
        await context.Cells
            .AddAsync(cell, ct);
    }

    public async Task UpdateAsync(CellEntity cell, CancellationToken ct)
    {
        await context.Cells
            .Where(c => c.Id == cell.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Value, cell.Value), cancellationToken: ct);
    }

    public async Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct)
    {
        await context.Cells
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Deleted, deleted), cancellationToken: ct);
    }
}