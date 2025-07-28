using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.EquipmentSheet;
using Microsoft.EntityFrameworkCore;
using Models.Entities.EquipmentSheet;

namespace Data.Repositories.EquipmentSheet;

public class EquipmentRowsRepository(AppDbContext context) : IEquipmentRowsRepository
{
    public async Task<List<EquipmentRowEntity>> GetListByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        return await context.EquipmentRows
            .AsNoTracking()
            .Where(r => r.EquipmentSheetId == equipmentSheetId)
            .Include(er => er.Row)
                .ThenInclude(r => r.Cells)
                    .ThenInclude(c => c.ColumnEntity)
            .OrderBy(er => er.Order)
            .ToListAsync(ct);
    }

    public async Task AddAsync(EquipmentRowEntity equipmentRowEntity, CancellationToken ct = default)
    {
        await context.EquipmentRows.AddAsync(equipmentRowEntity, ct);
    }

    public async Task<int> GetNextRowPositionAsync(Guid equipmentSheetId, CancellationToken ct = default)
    {
        var maxPosition = await context.EquipmentRows
            .AsNoTracking()
            .Where(er => er.EquipmentSheetId == equipmentSheetId)
            .Select(er => er.Row.Position)
            .DefaultIfEmpty(0)
            .MaxAsync(ct);

        return maxPosition + 1;
    }
    
    public async Task<int> GetNextRowOrderAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        var maxOrder = await context.EquipmentRows
            .AsNoTracking()
            .Where(er => er.EquipmentSheetId == equipmentSheetId)
            .Select(er => er.Order)
            .DefaultIfEmpty(0)
            .MaxAsync(ct);
        return maxOrder + 1;
    }
}