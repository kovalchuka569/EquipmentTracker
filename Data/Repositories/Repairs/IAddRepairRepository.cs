using System.Collections.ObjectModel;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Data.Repositories.Repairs;

public interface IAddRepairRepository
{
    Task<List<EquipmentDto>> GetDataAsync (string equipmentTableName);
    Task<int> SaveRepairAsync (RepairData repairData, string repairsTableName);
    Task InsertUsedMaterialsAsync (ObservableCollection<RepairConsumableDto> repairConsumableDtos, int repairId, string consumablesTableName);
}