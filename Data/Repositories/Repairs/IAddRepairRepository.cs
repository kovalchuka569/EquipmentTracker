using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Data.Repositories.Repairs;

public interface IAddRepairRepository
{
    Task<List<EquipmentDto>> GetDataAsync (string equipmentTableName);
    Task SaveRepairAsync (EquipmentDto equipment, ConsumableDto consumable);
}