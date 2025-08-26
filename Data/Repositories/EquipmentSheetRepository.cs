using Microsoft.EntityFrameworkCore;

using Models.Entities.EquipmentSheet;

using Data.ApplicationDbContext;
using Data.Interfaces;

namespace Data.Repositories;

public class EquipmentSheetRepository(AppDbContext context) : IEquipmentSheetRepository
{
    private const string EquipmentSheetNotFount = "Equipment sheet not found.";
    
    public async Task AddAsync(EquipmentSheetEntity entity, CancellationToken ct)
    {
        await context.AddAsync(entity, ct);
    }
    
    public async Task<string> GetColumnsJsonAsync(Guid equipmentSheetId, CancellationToken ct = default)
    {
        var sheet = await context.EquipmentSheets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == equipmentSheetId, ct);
        if (sheet != null)
        {
            return sheet.ColumnsJson;
        }
        return sheet?.ColumnsJson ?? string.Empty;
    }

    public async Task<string> GetRowsJsonAsync(Guid equipmentSheetId, CancellationToken ct = default)
    {
        var sheet = await context.EquipmentSheets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == equipmentSheetId, ct);

        if (sheet != null)
        {
            return sheet.RowsJson;
        }
        return sheet?.RowsJson ?? string.Empty;
    }

    public async Task UpdateColumnsAsync(Guid equipmentSheetId, string columnsJson, CancellationToken ct = default)
    {
        var sheet = await context.EquipmentSheets.FindAsync([equipmentSheetId], ct);

        if (sheet != null)
        {
            sheet.ColumnsJson = columnsJson;
        }
        else
        {
            throw new Exception(EquipmentSheetNotFount);
        }
    }

    public async Task UpdateRowsAsync(Guid equipmentSheetId, string rowsJson, CancellationToken ct = default)
    {
        var sheet = await context.EquipmentSheets.FindAsync([equipmentSheetId], ct);

        if (sheet != null)
        {
            sheet.RowsJson = rowsJson;
        }
        else
        {
            throw new Exception(EquipmentSheetNotFount);
        }
    }
}