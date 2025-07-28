using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.SummarySheet;
using Microsoft.EntityFrameworkCore;
using Models.Entities.SummarySheet;

namespace Data.Repositories.SummarySheet;

public class SummarySheetRowsRepository(AppDbContext context) : ISummarySheetRowsRepository
{
    public async Task<List<SummaryRowEntity>> GetListBySummarySheetIdAsync(Guid summarySheetId, CancellationToken ct)
    {
        return await context.SummaryRows
            .AsNoTracking()
            .Where(r => r.SummarySheetId == summarySheetId)
            .ToListAsync(ct);
    }

    public async Task AddAsync(SummaryRowEntity summaryRowEntity, CancellationToken ct)
    {
        await context.SummaryRows.AddAsync(summaryRowEntity, ct);
    }
}