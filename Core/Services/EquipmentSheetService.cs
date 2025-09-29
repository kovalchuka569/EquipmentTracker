using Common.Logging;
using Newtonsoft.Json;
using Core.Common.JsonConverters;
using Core.Services.Base;
using Core.Interfaces;
using Data.UnitOfWork;
using Models.Common.Table;
using Models.Common.Table.ColumnProperties;

namespace Core.Services;

public class EquipmentSheetService(IUnitOfWork unitOfWork, IAppLogger<EquipmentSheetService> logger)
    : DatabaseService<EquipmentSheetService>(unitOfWork, logger), IEquipmentSheetService
{
    
    private const string NoRowsFoundInTheProvidedSheetExMessage = "No rows found in the provided sheet";
    private const string NoColumnsFoundInTheProvidedSheetExMessage = "No columns found in the provided sheet";
    
    #region Interface realization
    
    public async Task<List<BaseColumnProperties>> GetColumnPropsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
       return await ExecuteInLoggerAsync(async () =>
       {
            var columnsJson = await UnitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
            return DeserializeColumns(columnsJson);
            
       }, nameof(GetColumnPropsByEquipmentSheetIdAsync), ct);
    }

    public async Task<List<RowModel>> GetActiveRowsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        return await ExecuteInLoggerAsync(async () =>
        {
            var rowsJson = await UnitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
            var rows = DeserializeRows(rowsJson);
            return rows.ToList();
            
        }, nameof(GetActiveRowsByEquipmentSheetIdAsync), ct);
    }

    public async Task InsertRowAsync(Guid equipmentSheetId, RowModel newRowModel, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var currentRowsJson = await UnitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
            var listRowsModel = DeserializeRows(currentRowsJson);

            foreach (var rowModel in listRowsModel)
            {
                rowModel.Position++;
            }
        
            listRowsModel.Add(newRowModel);

            var updatedRowsJson = SerializeRows(listRowsModel.OrderBy(r => r.Position).ToList());

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
                
            }, ct);
            
        }, nameof(InsertRowAsync), ct);
    }

    public async Task InsertRowsAsync(Guid equipmentSheetId, List<RowModel> newRowsModels, CancellationToken ct = default)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var currentRowsJson = await UnitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
            var listRowsModel = DeserializeRows(currentRowsJson);
            Console.WriteLine(listRowsModel.Count + "List rows models count");
        
            // Shift the positions of existing lines down
            foreach (var row in listRowsModel)
            {
                row.Position += newRowsModels.Count;
            }
        
            newRowsModels.Reverse();
        
            // Assign positions to new lines (top of list)
            var pos = 1;
            foreach (var newRow in newRowsModels)
            {
                newRow.Position = pos++;
            }
        
            // Add range of new rows
            listRowsModel.AddRange(newRowsModels);
        
            // Serialize in order by position
            var updatedRowsJson = SerializeRows(listRowsModel.OrderBy(r => r.Position).ToList());
        
            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
                
            }, ct);
            
        }, nameof(InsertRowsAsync), ct);
    }

    public async Task AddColumnPropsAsync(Guid equipmentSheetId, List<BaseColumnProperties> newColumnsProps, List<CellModel> newCellModels, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            if (newColumnsProps.Count == 0)
                return;
        
            // Add new column
            var columnsJson = await UnitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
            var columns = DeserializeColumns(columnsJson);
            columns.AddRange(newColumnsProps);
            var updatedColumnsJson = SerializeColumns(columns);

            var haveNewCells = newCellModels.Count != 0;
            var updatedRowsJson = string.Empty;

            if (haveNewCells)
            {
                // Add new cells for existing rows
                var listRowsModel = await GetActiveRowsByEquipmentSheetIdAsync(equipmentSheetId, ct);
        
                var rowsDictionary = listRowsModel.ToDictionary(row => row.Id);

                foreach (var newCellModel in newCellModels)
                {
                    if (rowsDictionary.TryGetValue(newCellModel.RowId, out var row))
                    {
                        row.Cells.Add(newCellModel);
                    }
                }
        
                updatedRowsJson = SerializeRows(listRowsModel);
            }

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);

                if (haveNewCells)
                {
                    await UnitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
                }
                
            }, ct);
            
        }, nameof(AddColumnPropsAsync), ct);
    }

    public async Task UpdateColumnPropsAsync(Guid equipmentSheetId, List<BaseColumnProperties> updatedColumnProps, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var columnsJson = await UnitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
            var columns = DeserializeColumns(columnsJson);

            foreach (var updatedColumn in updatedColumnProps)
            {
                var index = columns.FindIndex(c => c.Id == updatedColumn.Id);
                if (index == -1)
                    throw new InvalidOperationException(
                        $"Column with ID {updatedColumn.Id} not found in equipment sheet {equipmentSheetId}");

                columns[index] = updatedColumn;
            }
            var updatedColumnsJson = SerializeColumns(columns);

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
                
            }, ct);  
            
        }, nameof(UpdateColumnPropsAsync), ct);
    }

    public async Task UpdateColumnsPositionsAsync(Guid equipmentSheetId, Dictionary<Guid, int> columnNewPositions, CancellationToken ct = default)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var columnsJson = await UnitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
            var columnsModels = DeserializeColumns(columnsJson);
        
            columnsModels.Sort((a, b) =>
            {
                columnNewPositions.TryGetValue(a.Id, out var aPosition);
                columnNewPositions.TryGetValue(b.Id, out var bPosition);

                return aPosition.CompareTo(bPosition);
            });

            for (var i = 0; i < columnsModels.Count; i++)
            {
                columnsModels[i].Order = i;
            }
        
            var updatedColumnsJson = SerializeColumns(columnsModels);

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
                
            }, ct);
            
        }, nameof(UpdateColumnsPositionsAsync), ct);
    }

    public async Task UpdateColumnWidthAsync(Guid equipmentSheetId, Guid columnId, double newWidth, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var columnsJson = await UnitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
            var columnProps = DeserializeColumns(columnsJson);

            var columnToUpdate = columnProps.FirstOrDefault(c => c.Id == columnId);
            if (columnToUpdate == null)
            {
                throw new InvalidOperationException($"Column with ID {columnId} not found in equipment sheet {equipmentSheetId}");
            }
        
            columnToUpdate.HeaderWidth = newWidth;
        
            var updatedColumnsJson = SerializeColumns(columnProps);

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
                
            }, ct);
            
        }, nameof(UpdateColumnWidthAsync), ct);
    }

    public async Task UpdateRowAsync(Guid equipmentSheetId, Guid rowId, RowModel updatedRowModel, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var currentRowsJson = await UnitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
            var rows = DeserializeRows(currentRowsJson);
        
            var rowForUpdate = rows.FirstOrDefault(r => r.Id == rowId);
            if (rowForUpdate == null)
            {
                throw new InvalidOperationException($"Row with ID {rowId} not found for equipment sheet {equipmentSheetId}");
            }

            var index = rows.FindIndex(r => r.Id == rowId);
        
            rows[index] = updatedRowModel;
        
            var updatedRowsJson = SerializeRows(rows);

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
                
            }, ct);
            
        }, nameof(UpdateRowAsync), ct);
    }

    public async Task UpdateRowsAsync(Guid equipmentSheetId, List<RowModel> rowModels, bool sortByPosition, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            var rowsToSerialize = sortByPosition 
                ? rowModels.OrderBy(r => r.Position).ToList()
                : rowModels;
        
            var updatedRowsJson = SerializeRows(rowsToSerialize);

            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
                
            }, ct);
            
        }, nameof(UpdateRowsAsync), ct);
    }

    public async Task UpdateCellValueAsync(Guid equipmentSheetId, Guid cellId, object newValue, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            Console.WriteLine(newValue);
            var currentRowsJson = await UnitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
            var rows = DeserializeRows(currentRowsJson);
        
            var rowForUpdate = rows.FirstOrDefault(r => r.Cells.Any(c => c.Id == cellId));
            if (rowForUpdate is null)
                throw new InvalidOperationException($"Row containing cell with ID {cellId} not found in equipment sheet {equipmentSheetId}");
        
            var cellForUpdate = rowForUpdate.Cells.FirstOrDefault(c => c.Id == cellId);
            if (cellForUpdate is null)
                throw new InvalidOperationException($"Cell with ID {cellId} not found in row");
        
            cellForUpdate.Value = newValue;
        
            var updatedRowsJson = SerializeRows(rows);

            await ExecuteInTransactionAsync(async () =>
            {
                await UnitOfWork.EquipmentSheetRepository.UpdateAsync(
                    equipmentSheetId, 
                    s => s
                        .SetProperty(e => e.RowsJson, updatedRowsJson),
                    ct);
                
            }, ct);
            
        }, nameof(UpdateCellValueAsync), ct);
    }

    public async Task UpdateRowsMarkedForDeleteAsync(Guid equipmentSheetId, List<Guid> rowIds, bool isMarkedForDelete, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            
            var equipmentSheet = await UnitOfWork.EquipmentSheetRepository.GetByIdAsync(equipmentSheetId, ct);
            var rowModels = DeserializeRows(equipmentSheet.RowsJson);
            if (rowModels.Count == 0)
                throw new Exception(NoRowsFoundInTheProvidedSheetExMessage);

            foreach (var rowModel in rowModels.Where(rowModel => rowIds.Contains(rowModel.Id)))
                rowModel.IsMarkedForDelete = isMarkedForDelete;
            
            var updatedRowsJson = SerializeRows(rowModels);
            var hasMarkedForDeleteRows = rowModels.Any(r => r.IsMarkedForDelete);

            await ExecuteInTransactionAsync(async () =>
            {

                await UnitOfWork.EquipmentSheetRepository.UpdateAsync(
                    equipmentSheetId,
                    s => s
                        .SetProperty(e => e.RowsJson, updatedRowsJson)
                        .SetProperty(e => e.HasMarkedForDeleteRows, hasMarkedForDeleteRows),
                    ct
                );
                
            }, ct);

        }, nameof(UpdateRowsMarkedForDeleteAsync), ct);
    }

    public async Task UpdateColumnsMarkedForDeleteAsync(Guid equipmentSheetId, List<Guid> columnIds, bool isMarkedForDelete, CancellationToken ct)
    {
        await ExecuteInLoggerAsync(async () =>
        {
            var equipmentSheet = await UnitOfWork.EquipmentSheetRepository.GetByIdAsync(equipmentSheetId, ct);
            var columnProps = DeserializeColumns(equipmentSheet.ColumnsJson)
                ?? throw new Exception(NoColumnsFoundInTheProvidedSheetExMessage);

            foreach (var columnProp in columnProps.Where(columnProp => columnIds.Contains(columnProp.Id)))
                columnProp.MarkedForDelete = isMarkedForDelete;
            
            var updatedColumnsJson = SerializeColumns(columnProps);   
            var hasMarkedForDeleteColumns = columnProps.Any(c => c.MarkedForDelete);
            
            await ExecuteInTransactionAsync(async () =>
            {
                
                await UnitOfWork.EquipmentSheetRepository.UpdateAsync(
                    equipmentSheetId,
                    s => s
                        .SetProperty(e => e.ColumnsJson, updatedColumnsJson)
                        .SetProperty(e => e.HasMarkedForDeleteColumns, hasMarkedForDeleteColumns),
                    ct);
                
            }, ct);
            
        }, nameof(UpdateColumnsMarkedForDeleteAsync), ct);
    }
    
    #endregion
    
    #region Private methods

    private static List<BaseColumnProperties> DeserializeColumns(string columnsJson)
    {
        return string.IsNullOrEmpty(columnsJson)
            ? new()
            : JsonConvert.DeserializeObject<List<BaseColumnProperties>>(columnsJson, ColumnsSerializerSettings) 
              ?? new();
    }

    private static string SerializeColumns(List<BaseColumnProperties> columns)
    {
        return JsonConvert.SerializeObject(columns, Formatting.Indented, ColumnsSerializerSettings);
    }
    
    private static List<RowModel> DeserializeRows(string rowsJson)
    {
        var rows = JsonConvert.DeserializeObject<List<RowModel>>(rowsJson);

        if (rows is null) return [];
        foreach (var row in rows)
        { 
            foreach (var cell in row.Cells)
            {
                cell.RowId = row.Id;
            }
        }

        return rows;
    }
    
    private static string SerializeRows(List<RowModel> rows)
    {
        return JsonConvert.SerializeObject(rows, Formatting.Indented);
    }
    
    private static readonly JsonSerializerSettings ColumnsSerializerSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        TypeNameHandling = TypeNameHandling.None,
        Converters = { new ColumnPropertiesJsonConverter() }
    };
    
    #endregion
}