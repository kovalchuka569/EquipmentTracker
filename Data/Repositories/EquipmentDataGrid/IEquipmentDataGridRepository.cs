using Models.Equipment;

namespace Data.Repositories.EquipmentDataGrid;

public interface IEquipmentDataGridRepository
{
   /* Task<List<string>> GetColumnNamesAsync(string equipmentTableName);
    Task<List<EquipmentDto>> GetEquipmentListAsync(string equipmentTableName);
    Task<int> InsertEquipmentAsync(EquipmentDto equipment, string equipmentTableName);
    Task UpdateEquipmentAsync(EquipmentDto equipment, string equipmentTableName);
    Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName);
    Task MakeDataCopyAsync(int equipmentId, string equipmentTableName);
    Task<List<SparePartDto>> GetSparePartListAsync(int equipmentId, string sparePartTableName);
    Task<int> InsertSparePartAsync(SparePartDto sparePart, string sparePartTableName);
    Task UpdateSparePartAsync(SparePartDto sparePart, string sparePartTableName);*/
    
    // ============================================================================
    
    Task<List<ColumnDto>> GetColumnsAsync(int tableId);
    Task<List<EquipmentDto>> GetRowsAsync(int tableId);
    
    /// <summary>
    /// Searches the database for an existing MappingName
    /// </summary>
    /// <param name="headerText">HeaderText</param>
    /// <returns>Existing MappingName if match found or new MappingName if match not found</returns>
    Task<string?> GetMappingName(string headerText);
    
    /// <summary>
    /// Update header_text in header dictionary
    /// </summary>
    /// <param name="headerText">HeaderText</param>
    /// <param name="mappingName">MappingName</param>
    Task UpdateHeaderTextInDictionaryAsync(string headerText, string mappingName);
    
    Task UpdateRowsAsync(IDictionary<string, object> rows, int id);
    Task UpdateColumnAsync(ColumnDto column);
    Task UpdateColumnPositionAsync(Dictionary<int, int> columnPositions, int tableId);
    Task UpdateColumnWidthAsync(Dictionary<int, double> columnWidths, int tableId);
    Task<int> CreateColumnAsync(ColumnSettings column, int tableId);
    Task<int> AddNewRowAsync(EquipmentDto equipment);
}