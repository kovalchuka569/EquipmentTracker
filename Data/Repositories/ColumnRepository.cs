using Data.ApplicationDbContext;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Entities.Table;

namespace Data.Repositories;

public class ColumnRepository(AppDbContext context) : IColumnRepository
{
    public async Task<ColumnEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await context.Columns
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken: ct) 
               ?? throw new InvalidOperationException("Column not found");
    }

    public async Task AddAsync(ColumnEntity column, CancellationToken ct)
    {
        await context.Columns.AddAsync(column, ct);
    }

    public async Task UpdateAsync(ColumnEntity column, CancellationToken ct)
    {
        await context.Columns
            .ExecuteUpdateAsync(c => c
                .SetProperty(col => col.Settings , column.Settings), cancellationToken: ct);
    }

    public async Task UpdateDeletedByIdAsync(Guid id, bool deleted, CancellationToken ct)
    {
        await context.Columns
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s.
                SetProperty(c => c.Deleted, deleted), cancellationToken: ct);
    }
}