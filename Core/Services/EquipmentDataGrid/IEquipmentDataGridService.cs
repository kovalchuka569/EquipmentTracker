using System.Collections.ObjectModel;
using Models.EquipmentDataGrid;

namespace Core.Services.EquipmentDataGrid;

public interface IEquipmentDataGridService
{
    Task<Dictionary<string, bool>> GetVisibleColumnsAsync(string equipmentTableName);
    Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName);
    Task<int> InsertEquipmentAsync(EquipmentItem equipment, string equipmentTableName);
    Task UpdateEquipmentAsync(EquipmentItem equipment, string equipmentTableName);
    Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName);
    Task MakeDataCopyAsync(int equipmentId, string equipmentTableName);
    Task<ObservableCollection<SparePartItem>> GetSparePartItemAsync(int equipmentId, string sparePartTableName);
}