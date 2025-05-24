using System.Collections.ObjectModel;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Core.Services.RepairsDataGrid;

public interface IAddRepairService
{
    Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName);
    Task<int> SaveRepairAsync(RepairData repairData, string equipmentTableName);
    Task InsertUsedMaterialsAsync(List<RepairConsumableItem> repairConsumableItems, int repairId, string repairConsumablesTableName);
    Task<ObservableCollection<RepairConsumableItem>> LoadUsedMaterialsAsync(string repairConsumablesTableName, int repairId);
    Task UpdateRepairAsync(RepairData repairData, string repairTableName, int repairId);
}