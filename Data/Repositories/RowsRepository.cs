using Data.ApplicationDbContext;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Entities.Table;

namespace Data.Repositories;

public class RowsRepository(AppDbContext context) : IRowsRepository
{
    public async Task<RowEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Rows
                   .AsNoTracking()
                   .FirstOrDefaultAsync(r=> r.Id == id, cancellationToken: ct) 
               ?? throw new InvalidOperationException("Row not found");
    }
    
    public async Task AddAsync(RowEntity row, CancellationToken ct)
    {
        await context.Rows
            .AddAsync(row, ct);
    }

    public async Task UpdateAsync(RowEntity row, CancellationToken ct)
    {
        await context.Rows
            .Where(r => r.Id == row.Id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Position, row.Position), cancellationToken: ct);
    }

    public async Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct)
    {
        await context.Rows
            .Where(r => r.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Deleted, deleted), cancellationToken: ct);
    }
}