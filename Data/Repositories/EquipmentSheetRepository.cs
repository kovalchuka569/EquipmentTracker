using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Data.Entities;
using Data.ApplicationDbContext;
using Data.Interfaces;
using Microsoft.EntityFrameworkCore.Query;

namespace Data.Repositories;

public class EquipmentSheetRepository(AppDbContext context) : IEquipmentSheetRepository
{
    private const string EquipmentSheetNotFound = "Equipment sheet not found.";

    public async Task<EquipmentSheetEntity> GetByIdAsync(Guid id, CancellationToken ct)
    {
       var equipmentSheet = await context.EquipmentSheets
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e
                    .Id == id, cancellationToken: ct);
       
       if (equipmentSheet == null)
           throw new Exception(EquipmentSheetNotFound);
       
       return equipmentSheet;
    }

    public async Task<EquipmentSheetEntity> GetByIdWithTrackingAsync(Guid id, CancellationToken ct = default)
    {
        var equipmentSheet = await context.EquipmentSheets
            .FirstOrDefaultAsync(e => e
                .Id == id, cancellationToken: ct);
       
        if (equipmentSheet == null)
            throw new Exception(EquipmentSheetNotFound);
       
        return equipmentSheet;
    }

    public async Task AddAsync(EquipmentSheetEntity entity, CancellationToken ct)
    {
        await context.AddAsync(entity, ct);
    }

    public async Task UpdateAsync(EquipmentSheetEntity entity, CancellationToken ct)
    {
        var existing = await context.EquipmentSheets
            .FirstOrDefaultAsync(e => e.Id == entity.Id, ct)
            ?? throw new Exception(EquipmentSheetNotFound);
        
        context.Entry(existing).CurrentValues.SetValues(entity);
    }

    public async Task UpdateAsync(Guid id, Expression<Func<SetPropertyCalls<EquipmentSheetEntity>, 
        SetPropertyCalls<EquipmentSheetEntity>>> updateExpression, 
        CancellationToken ct = default)
    {
        await context.EquipmentSheets
            .Where(e => e.Id == id)
            .ExecuteUpdateAsync(updateExpression, ct);
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
            throw new Exception(EquipmentSheetNotFound);
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
            throw new Exception(EquipmentSheetNotFound);
        }
    }
    
}