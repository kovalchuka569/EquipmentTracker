using Models.Entities.EquipmentSheet;

namespace Data.Repositories.Interfaces.EquipmentSheet;

public interface IEquipmentRowsRepository
{
    /// <summary>
    /// Get rows entities list by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    Task<List<EquipmentRowEntity>> GetListByEquipmentSheetIdAsync (Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Add a new row in the equipment sheet.
    /// </summary>
    /// <param name="equipmentRowEntity">Equipment row entity</param>
    /// <param name="ct">Cancellation token</param>
    Task AddAsync (EquipmentRowEntity equipmentRowEntity, CancellationToken ct = default);
    
    /// <summary>
    /// Get next row position by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Next row position</returns>
    Task<int> GetNextRowPositionAsync (Guid equipmentSheetId, CancellationToken ct = default);
    
    /// <summary>
    /// Get next row order by equipment sheet id.
    /// </summary>
    /// <param name="equipmentSheetId">Equipment sheet id</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>Next row order</returns>
    Task<int> GetNextRowOrderAsync (Guid equipmentSheetId, CancellationToken ct = default);
}