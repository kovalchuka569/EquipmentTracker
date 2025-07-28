using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.SummarySheet;
using Microsoft.EntityFrameworkCore;
using Models.Entities.SummarySheet;
using Syncfusion.Data.Extensions;

namespace Data.Repositories.SummarySheet;

public class SummarySheetsRepository(AppDbContext context) : ISummarySheetsRepository  
{
    public async Task<SummarySheetEntity> GetByIdAsync(Guid summarySheetId, CancellationToken ct = default)
    {
        return await context.SummarySheets
            .AsNoTracking()
            .Where(ss => ss.Id == summarySheetId)
            .FirstOrDefaultAsync(ct) 
               ?? throw new Exception("Summary sheet not found");
    }

    public async Task AddAsync(SummarySheetEntity summarySheetEntity, CancellationToken ct = default)
    {
        await context.SummarySheets
            .AddAsync(summarySheetEntity, ct);
    }
}