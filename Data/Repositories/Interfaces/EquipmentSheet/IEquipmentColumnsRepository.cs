using Models.Entities.EquipmentSheet;

namespace Data.Repositories.Interfaces.EquipmentSheet;

public interface IEquipmentColumnsRepository
{
    /// <summary>
    /// Get columns entities list by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>List of equipment columns entities</returns>
    Task<List<EquipmentColumnEntity>> GetListByEquipmentSheetIdAsync (Guid equipmentSheetId, CancellationToken ct = default);

    /// <summary>
    /// Get column mapping names and id by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Dictionary of mapping name columns and ids</returns>
    Task<Dictionary<string, Guid>> GetMappingNamesAndIdsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Add new equipment column entity.
    /// </summary>
    /// <param name="equipmentColumnEntity">Equipment column entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync(EquipmentColumnEntity equipmentColumnEntity, CancellationToken ct = default);
}