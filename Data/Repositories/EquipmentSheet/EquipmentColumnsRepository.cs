using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.EquipmentSheet;
using Microsoft.EntityFrameworkCore;
using Models.Entities.EquipmentSheet;

namespace Data.Repositories.EquipmentSheet;

public class EquipmentColumnsRepository(AppDbContext context) : IEquipmentColumnsRepository
{
    public async Task<List<EquipmentColumnEntity>> GetListByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        return await context.EquipmentColumns
            .AsNoTracking()
            .Where(ec => ec.EquipmentSheetId == equipmentSheetId)
            .Include(ec => ec.ColumnEntity)
            .ToListAsync(ct);
    }

    public async Task<Dictionary<string, Guid>> GetMappingNamesAndIdsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default)
    {
        return await context.EquipmentColumns
            .AsNoTracking()
            .Where(ec => ec.EquipmentSheetId == equipmentSheetId)
            .Include(ec => ec.ColumnEntity) 
            .ToDictionaryAsync(
                ec => ec.ColumnEntity.Settings.MappingName, 
                ec => ec.ColumnId,
                ct
            );
    }

    public async Task AddAsync(EquipmentColumnEntity equipmentColumnEntity, CancellationToken ct = default)
    {
        await context.EquipmentColumns.AddAsync(equipmentColumnEntity, ct);
    }
}