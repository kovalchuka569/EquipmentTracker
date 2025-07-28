using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.SummarySheet;
using Microsoft.EntityFrameworkCore;
using Models.Entities.SummarySheet;
using System.ComponentModel.Design.Serialization;

namespace Data.Repositories.SummarySheet;

public class SummarySheetColumnsRepository(AppDbContext context) : ISummarySheetColumnsRepository
{
    public async Task<List<SummaryColumnEntity>> GetListBySummarySheetIdAsync(Guid summaryId, CancellationToken ct)
    {
        return await context.SummaryColumns
            .Where(sc => sc.SummarySheetId == summaryId)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    public async Task AddAsync(SummaryColumnEntity entity, CancellationToken ct)
    {
        await context.SummaryColumns.AddAsync(entity, ct);
    }
}