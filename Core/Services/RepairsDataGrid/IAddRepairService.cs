using System.Collections.ObjectModel;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;

namespace Core.Services.RepairsDataGrid;

public interface IAddRepairService
{
    Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName);
    Task<int> SaveRepairAsync(RepairData repairData, string equipmentTableName);
    Task InsertUsedMaterialsAsync(ObservableCollection<RepairConsumableItem> repairConsumableItems, int repairId, string consumablesTableName);
}