using System.Collections.ObjectModel;
using Models.Equipment;

namespace Core.Services.EquipmentDataGrid;

public interface IEquipmentDataGridService
{
   /* Task<Dictionary<string, bool>> GetVisibleColumnsAsync(string equipmentTableName);
    Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName);
    Task<int> InsertEquipmentAsync(EquipmentItem equipment, string equipmentTableName);
    Task UpdateEquipmentAsync(EquipmentItem equipment, string equipmentTableName);
    Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName);
    Task MakeDataCopyAsync(int equipmentId, string equipmentTableName);
    Task<ObservableCollection<SparePartItem>> GetSparePartItemAsync(int equipmentId, string sparePartTableName);
    Task<int> InsertSparePartAsync(SparePartItem sparePart, string sparePartTableName);
    Task UpdateSparePartAsync(SparePartItem sparePart, string sparePartTableName);*/
    
    // ----------------------------------------------------------------------------------------------------------------
    Task<ObservableCollection<ColumnItem>> GetColumnsAsync(int tableId);
    Task<List<EquipmentItem>> GetRowsAsync(int tableId);
    Task UpdateColumnAsync(ColumnItem column);
    Task UpdateColumnPositionAsync(Dictionary<int, int> columnPosition, int tableId);
    Task UpdateColumnWidthAsync(Dictionary<int, double> columnWidths, int tableId);
    Task<int> AddColumnAsync(ColumnSettings column, int tableId);
    Task<int> AddNewRowAsync(EquipmentItem equipment);
    Task UpdateRowsAsync(IDictionary<string, object> rows, int id);
}