using System.Collections.ObjectModel;
using Models.EquipmentDataGrid;

namespace Data.Repositories.EquipmentDataGrid;

public interface IEquipmentDataGridRepository
{
    Task<List<string>> GetColumnNamesAsync(string equipmentTableName);
    Task<List<EquipmentDto>> GetEquipmentListAsync(string equipmentTableName);
    Task<int> InsertEquipmentAsync(EquipmentDto equipment, string equipmentTableName);
    Task UpdateEquipmentAsync(EquipmentDto equipment, string equipmentTableName);
    Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName);
    Task MakeDataCopyAsync(int equipmentId, string equipmentTableName);
    Task<List<SparePartDto>> GetSparePartListAsync(int equipmentId, string sparePartTableName);
}