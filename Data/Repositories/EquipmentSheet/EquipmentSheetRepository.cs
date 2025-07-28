using Data.ApplicationDbContext;
using Data.Repositories.Interfaces.EquipmentSheet;
using Models.Entities.EquipmentSheet;

namespace Data.Repositories.EquipmentSheet;

public class EquipmentSheetRepository(AppDbContext context) : IEquipmentSheetRepository
{
    public async Task AddAsync(EquipmentSheetEntity entity, CancellationToken cancellationToken)
    {
        await context.AddAsync(entity, cancellationToken);
    }
}