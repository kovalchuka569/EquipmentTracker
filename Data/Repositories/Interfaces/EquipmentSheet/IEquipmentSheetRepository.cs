using Models.Entities.EquipmentSheet;

namespace Data.Repositories.Interfaces.EquipmentSheet;

public interface IEquipmentSheetRepository
{
    /// <summary>
    /// Adds a new equipment sheet entity to the database.
    /// </summary>
    /// <param name="entity">Equipment sheet entity</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddAsync(EquipmentSheetEntity entity, CancellationToken cancellationToken);
}