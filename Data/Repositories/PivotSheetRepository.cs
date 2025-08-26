using Microsoft.EntityFrameworkCore;

using Models.Entities.PivotSheet;

using Data.ApplicationDbContext;
using Data.Interfaces;


namespace Data.Repositories;

public class PivotSheetRepository(AppDbContext context) : IPivotSheetRepository  
{
    public async Task<PivotSheetEntity> GetByIdAsync(Guid summarySheetId, CancellationToken ct = default)
    {
        return await context.PivotSheets
            .AsNoTracking()
            .Where(ss => ss.Id == summarySheetId)
            .FirstOrDefaultAsync(ct) 
               ?? throw new Exception("Summary sheet not found");
    }

    public async Task AddAsync(PivotSheetEntity pivotSheetEntity, CancellationToken ct = default)
    {
        await context.PivotSheets
            .AddAsync(pivotSheetEntity, ct);
    }
}