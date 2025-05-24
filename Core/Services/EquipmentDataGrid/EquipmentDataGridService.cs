using System.Collections.ObjectModel;
using Common.Logging;
using Data.Repositories.EquipmentDataGrid;
using Models.EquipmentDataGrid;

namespace Core.Services.EquipmentDataGrid;

public class EquipmentDataGridService : IEquipmentDataGridService
{
    private readonly IEquipmentDataGridRepository _repository;

    public EquipmentDataGridService(IEquipmentDataGridRepository repository,
        IAppLogger<EquipmentDataGridService> logger)
    {
        _repository = repository;
    }
    
    public async Task<Dictionary<string, bool>> GetVisibleColumnsAsync(string equipmentTableName)
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
            SparePartName = s.SparePartName,
            SparePartCategory = s.SparePartCategory,
            EquipmentId = equipmentId,
            SparePartQuantity = s.SparePartQuantity,
            SparePartUnit = s.SparePartUnit,
            SparePartSerialNumber = s.SparePartSerialNumber,
            SparePartNotes = s.SparePartNotes
        });
        return new ObservableCollection<SparePartItem>(sparePartsItems);
    }
}