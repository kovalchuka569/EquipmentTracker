using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.EquipmentDataGrid;
using Models.Equipment;
using Syncfusion.Linq;

namespace Core.Services.EquipmentDataGrid;

public class EquipmentDataGridService : IEquipmentDataGridService
{
    private readonly IEquipmentDataGridRepository _repository;

    public EquipmentDataGridService(IEquipmentDataGridRepository repository,
        IAppLogger<EquipmentDataGridService> logger)
    {
        _repository = repository;
    }
    
    /*public async Task<Dictionary<string, bool>> GetVisibleColumnsAsync(string equipmentTableName)
    {
        var columnsFromDb = await _repository.GetColumnNamesAsync(equipmentTableName);

        var visibleColumns = EquipmentColumnInfo.MappingToDbColumn.ToDictionary(
            mapping => mapping.Key,
            mapping => columnsFromDb.Contains(mapping.Value, StringComparer.OrdinalIgnoreCase));
           
        return visibleColumns;
    }

    public async Task<ObservableCollection<EquipmentItem>> GetEquipmentItemsAsync(string equipmentTableName)
    {
        var equipmentFromDb = await _repository.GetEquipmentListAsync(equipmentTableName);

        var equipmentItems = equipmentFromDb.Select(e => new EquipmentItem
        {
            Id = e.Id,
            InventoryNumber = e.InventoryNumber,
            Brand = e.Brand,
            Model = e.Model,
            Category = e.Category,
            SerialNumber = e.SerialNumber,
            Class = e.Class,
            Year = e.Year,
            Width = e.Width,
            Height = e.Height,
            Length = e.Length,
            Weight = e.Weight,
            Floor = e.Floor,
            Department = e.Department,
            Room = e.Room,
            Consumption = e.Consumption,
            Voltage = e.Voltage,
            Water = e.Water,
            Air = e.Air,
            BalanceCost = e.BalanceCost,
            Notes = e.Notes,
            ResponsiblePerson = e.ResponsiblePerson
        });
        return new ObservableCollection<EquipmentItem>(equipmentItems);
    }

    public async Task<int> InsertEquipmentAsync(EquipmentItem equipment, string equipmentTableName)
    {
        var equipmentDto = new EquipmentDto
        {
            InventoryNumber = equipment.InventoryNumber,
            Brand = equipment.Brand,
            Model = equipment.Model,
            Category = equipment.Category,
            SerialNumber = equipment.SerialNumber,
            Class = equipment.Class,
            Year = equipment.Year,
            Height = equipment.Height,
            Length = equipment.Length,
            Width = equipment.Width,
            Weight = equipment.Weight,
            Floor = equipment.Floor,
            Department = equipment.Department,
            Room = equipment.Room,
            Consumption = equipment.Consumption,
            Voltage = equipment.Voltage,
            Water = equipment.Water,
            Air = equipment.Air,
            BalanceCost = equipment.BalanceCost,
            Notes = equipment.Notes,
            ResponsiblePerson = equipment.ResponsiblePerson
        };
        
        return await _repository.InsertEquipmentAsync(equipmentDto, equipmentTableName);
    }

    public async Task UpdateEquipmentAsync(EquipmentItem equipment, string equipmentTableName)
    {
        var equipmentDto = new EquipmentDto
        {
            Id = equipment.Id,
            InventoryNumber = equipment.InventoryNumber,
            Brand = equipment.Brand,
            Model = equipment.Model,
            Category = equipment.Category,
            SerialNumber = equipment.SerialNumber,
            Class = equipment.Class,
            Year = equipment.Year,
            Height = equipment.Height,
            Length = equipment.Length,
            Width = equipment.Width,
            Weight = equipment.Weight,
            Floor = equipment.Floor,
            Department = equipment.Department,
            Room = equipment.Room,
            Consumption = equipment.Consumption,
            Voltage = equipment.Voltage,
            Water = equipment.Water,
            Air = equipment.Air,
            BalanceCost = equipment.BalanceCost,
            Notes = equipment.Notes,
            ResponsiblePerson = equipment.ResponsiblePerson
        };
        
        await _repository.UpdateEquipmentAsync(equipmentDto, equipmentTableName);
    }

    public async Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName)
    {
        await _repository.WriteOffEquipmentAsync(equipmentId, equipmentTableName);
    }

    public async Task MakeDataCopyAsync(int equipmentId, string equipmentTableName)
    {
        await _repository.MakeDataCopyAsync(equipmentId, equipmentTableName);
    }

    public async Task<ObservableCollection<SparePartItem>> GetSparePartItemAsync(int equipmentId, string sparePartTableName)
    {
        var sparePartsFromDb = await _repository.GetSparePartListAsync(equipmentId, sparePartTableName);
        var sparePartsItems = sparePartsFromDb.Select(s => new SparePartItem
        {
            Id = s.Id,
            EquipmentId = s.EquipmentId,
            SparePartName = s.SparePartName,
            SparePartCategory = s.SparePartCategory,
            SparePartQuantity = s.SparePartQuantity,
            SparePartUnit = s.SparePartUnit,
            SparePartSerialNumber = s.SparePartSerialNumber,
            SparePartNotes = s.SparePartNotes
        });
        return new ObservableCollection<SparePartItem>(sparePartsItems);
    }

    public async Task<int> InsertSparePartAsync(SparePartItem sparePart, string sparePartTableName)
    {
        var sparePartDto = new SparePartDto
        {
            EquipmentId = sparePart.EquipmentId,
            SparePartName = sparePart.SparePartName,
            SparePartCategory = sparePart.SparePartCategory,
            SparePartNotes = sparePart.SparePartNotes,
            SparePartQuantity = sparePart.SparePartQuantity,
            SparePartUnit = sparePart.SparePartUnit,
            SparePartSerialNumber = sparePart.SparePartSerialNumber,
        };
        return await _repository.InsertSparePartAsync(sparePartDto, sparePartTableName);
    }

    public async Task UpdateSparePartAsync(SparePartItem sparePart, string sparePartTableName)
    {
        var sparePartDto = new SparePartDto
        {
            Id = sparePart.Id,
            SparePartName = sparePart.SparePartName,
            SparePartCategory = sparePart.SparePartCategory,
            SparePartNotes = sparePart.SparePartNotes,
            SparePartQuantity = sparePart.SparePartQuantity,
            SparePartUnit = sparePart.SparePartUnit,
            SparePartSerialNumber = sparePart.SparePartSerialNumber,
        };
        await _repository.UpdateSparePartAsync(sparePartDto, sparePartTableName);
    }*/
    
    // ------------------------------------------------------------------------------------------------

    public async Task<ObservableCollection<ColumnItem>> GetColumnsAsync(int tableId)
    {
        var columnsFromDb = await _repository.GetColumnsAsync(tableId);
        return columnsFromDb.Select(c => new ColumnItem
        {
            Id = c.Id,
            TableId = c.TableId,
            Settings = c.Settings
        }).ToObservableCollection();
    }

    public async Task<List<EquipmentItem>> GetRowsAsync(int tableId)
    {
        var equipmentsFromDb = await _repository.GetRowsAsync(tableId);
        return equipmentsFromDb.Select(e => new EquipmentItem
        {
            Id = e.Id,
            TableId = e.TableId,
            RowIndex = e.RowIndex,
            Data = e.Data,
        }).ToList();
    }

    public async Task UpdateColumnAsync(ColumnItem column)
    {
        var columnDto = new ColumnDto
        {
            Id = column.Id,
            TableId = column.TableId,
            Settings = column.Settings
        };
        await _repository.UpdateColumnAsync(columnDto);
    }

    public async Task<string> GetMappingName(string headerText)
    {
       return await _repository.GetMappingName(headerText);
    }

    public async Task UpdateHeaderTextInDictionaryAsync(string headerText, string mappingName)
    {
        await _repository.UpdateHeaderTextInDictionaryAsync(headerText, mappingName);
    }

    public async Task UpdateColumnPositionAsync(Dictionary<int, int> columnPositions, int tableId)
    {
        await _repository.UpdateColumnPositionAsync(columnPositions, tableId);
    }

    public async Task UpdateColumnWidthAsync(Dictionary<int, double> columnWidths, int tableId)
    {
        await _repository.UpdateColumnWidthAsync(columnWidths, tableId);
    }

    public async Task<int> AddColumnAsync(ColumnSettings column, int tableId)
    {
        int id = await _repository.CreateColumnAsync(column, tableId);
        return id;
    }
    
    public async Task<int> AddNewRowAsync(EquipmentItem equipment)
    {
        var equipmentDto = new EquipmentDto
        {
            Id = equipment.Id,
            RowIndex = equipment.RowIndex,
            TableId = equipment.TableId,
            Data = equipment.Data
        };
        
        return await _repository.AddNewRowAsync(equipmentDto);
    }

    public async Task UpdateRowsAsync(IDictionary<string, object> rows, int id)
    {
        await _repository.UpdateRowsAsync(rows, id);
    }
}