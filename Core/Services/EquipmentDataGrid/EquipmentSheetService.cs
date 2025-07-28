using System.Collections.ObjectModel;
using Core.Services.Common;
using Data.UnitOfWork;
using Models.Entities.EquipmentSheet;
using Models.Entities.Table;
using Models.Equipment;
using Models.Equipment.ColumnSettings;

namespace Core.Services.EquipmentDataGrid;

public class EquipmentSheetService(IUnitOfWork unitOfWork) : IEquipmentSheetService, IAsyncDisposable
{
    public async Task<ObservableCollection<ColumnItem>> GetColumnsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        
        var equipmentColumnEntities = await unitOfWork.EquipmentColumnsRepository.GetListByEquipmentSheetIdAsync(equipmentSheetId, ct);
        
        
        var result = new ObservableCollection<ColumnItem>();
        
        foreach (var column in equipmentColumnEntities)
        {
            var columnEntity = column.ColumnEntity;
            var columnSettings = column.ColumnEntity.Settings;
            var displayColumnSettings = new ColumnSettingsParser().ToColumnSettingsDisplayModel(columnSettings);
            
            var columnItem = new ColumnItem
            {
                Id = columnEntity.Id,
                EquipmentSheetId = column.EquipmentSheetId,
                Settings = displayColumnSettings
            };
            
            result.Add(columnItem);
        }

        return result;
    }

    public async Task<ObservableCollection<RowItem>> GetRowsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        
        var equipmentRowEntities = await unitOfWork.EquipmentRowsRepository.GetListByEquipmentSheetIdAsync(equipmentSheetId, ct);

        var result = new ObservableCollection<RowItem>();
        
        foreach (var er in equipmentRowEntities)
        {
            var rowItem = new RowItem
            {
                Id = er.Row.Id,
                Data = new Dictionary<string, object?>()
            };
            
            var rowData = new Dictionary<string, object?>();

            foreach (var cell in er.Row.Cells)
            {
                rowItem.Data[cell.ColumnEntity.Settings.MappingName] = cell.Value;
            }
            rowItem.Data = rowData;
            result.Add(rowItem);
        }

        return result;
    }

    public async Task InsertRowAsync(Guid equipmentSheetId, RowItem rowItem, CancellationToken ct)
    {
        var rowEntity = new RowEntity
        {
            Id = rowItem.Id,
            Position = await unitOfWork.EquipmentRowsRepository.GetNextRowPositionAsync(equipmentSheetId, ct),
            Deleted = false,
            Cells = new List<CellEntity>()
        };

        var equipmentRowEntity = new EquipmentRowEntity
        {
            RowId = rowEntity.Id,
            EquipmentSheetId = equipmentSheetId,
            Order = await unitOfWork.EquipmentRowsRepository.GetNextRowOrderAsync(equipmentSheetId, ct),
            Row = rowEntity
        };
        
        var columnMappingNameToIds = await unitOfWork.EquipmentColumnsRepository.GetMappingNamesAndIdsByEquipmentSheetIdAsync(equipmentSheetId, ct);

        foreach (var dataEntry in rowItem.Data)
        {
            var mappingName = dataEntry.Key;
            var cellValue = dataEntry.Value?.ToString() ?? string.Empty;

            if (columnMappingNameToIds.TryGetValue(mappingName, out var columnId))
            {
                var cellEntity = new CellEntity
                {
                    Id = Guid.NewGuid(),
                    RowId = rowEntity.Id,
                    ColumnId = columnId,
                    Value = cellValue,
                    Deleted = false
                };
                rowEntity.Cells.Add(cellEntity);
            }
        }

        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.RowsRepository.AddAsync(rowEntity, ct);
        await unitOfWork.EquipmentRowsRepository.AddAsync(equipmentRowEntity, ct);
    }

    public async Task InsertColumnAsync(ColumnItem columnItem, CancellationToken ct = default)
    {
        var columnEntity = new ColumnEntity
        {
            Id = columnItem.Id,
            Deleted = false,
            Settings = new ColumnSettingsParser().ToColumnSettingsBase(columnItem.Settings),
            Cells = new List<CellEntity>(),
        };

        var equipmentColumnEntity = new EquipmentColumnEntity
        {
            ColumnId = columnEntity.Id,
            EquipmentSheetId = columnItem.EquipmentSheetId,
            ColumnEntity = columnEntity,
        };
        await unitOfWork.BeginTransactionAsync(ct);
        await unitOfWork.ColumnRepository.AddAsync(columnEntity, ct);
        await unitOfWork.EquipmentColumnsRepository.AddAsync(equipmentColumnEntity, ct);
        await unitOfWork.CommitAsync(ct);
    }

    public Task UpdateRowAsync(RowItem rowItem, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveRowAsync(Guid rowId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateColumnAsync(ColumnItem column)
    {
        throw new NotImplementedException();
    }

    public Task UpdateColumnPositionAsync(Dictionary<Guid, int> columnPosition, Guid tableId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateColumnWidthAsync(Dictionary<Guid, double> columnWidths, Guid tableId)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> AddColumnAsync(ColumnSettingsDisplayModel column, Guid tableId)
    {
        throw new NotImplementedException();
    }

    public Task<List<Guid>> InsertRowsAsync(Guid tableId, List<(Guid position, Dictionary<string, object?> data)> rows, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveItemsAsync(List<Guid> rowsId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public Task RemoveColumnAsync(Guid columnId, CancellationToken ct = default)
    {
        throw new NotImplementedException();
    }

    public async ValueTask DisposeAsync()
    {
        await unitOfWork.DisposeAsync();
    }
}