using Newtonsoft.Json;

using Core.Common.JsonConverters;
using Core.Interfaces;
using Data.UnitOfWork;
using Models.Common.Table;

namespace Core.Services;

public class EquipmentSheetService(IUnitOfWork unitOfWork) : IEquipmentSheetService, IAsyncDisposable
{
    private bool _disposed;

    private static readonly JsonSerializerSettings ColumnsSerializerSettings = new()
    {
        TypeNameHandling = TypeNameHandling.None,
        Converters = new List<JsonConverter>
        {
            new FontFamilyJsonConverter(),
            new ColorJsonConverter(),
            new FontWeightJsonConverter(),
            new ThicknessJsonConverter(),
            new ColumnValidationRulesConverter(),
            new ColumnSpecificSettingsConverter()
        }
    };
    
    private static List<ColumnModel> DeserializeColumns(string columnsJson)
    {
        return string.IsNullOrEmpty(columnsJson)
            ? new()
            : JsonConvert.DeserializeObject<List<ColumnModel>>(columnsJson, ColumnsSerializerSettings) 
              ?? new();
    }

    private static string SerializeColumns(List<ColumnModel> columns)
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

    private async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken ct)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync(ct);
            await action();
            await unitOfWork.CommitAsync(ct);
        }
        catch (Exception)
        {
            await unitOfWork.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<List<ColumnModel>> GetActiveColumnsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columns = DeserializeColumns(columnsJson);
        return columns.Where(c => !c.SoftDeleted).ToList();
    }

    public async Task<List<RowModel>> GetActiveRowsByEquipmentSheetIdAsync(Guid equipmentSheetId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var rowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        var rows = DeserializeRows(rowsJson);
        return rows.Where(r => !r.Deleted).ToList();
    }

    public async Task InsertRowAsync(Guid equipmentSheetId, RowModel newRowModel, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        var listRowsModel = DeserializeRows(currentRowsJson);

        foreach (var rowModel in listRowsModel)
        {
            rowModel.Position++;
        }
        
        listRowsModel.Add(newRowModel);

        var updatedRowsJson = SerializeRows(listRowsModel.OrderBy(r => r.Position).ToList());

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task InsertRowsAsync(Guid equipmentSheetId, List<RowModel> newRowsModels, CancellationToken ct = default)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
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
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task InsertColumnAsync(Guid equipmentSheetId, ColumnModel columnModel, List<CellModel> newCellModels, CancellationToken ct)
    {
        // Add new column
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columns = DeserializeColumns(columnsJson);
        columns.Add(columnModel);
        var updatedColumnsJson = SerializeColumns(columns);

        var haveNewCells = newCellModels.Count != 0;
        var updatedRowsJson = string.Empty;

        if (haveNewCells)
        {
            // Add new cells for existing rows
            await unitOfWork.EnsureInitializedForReadAsync(ct);
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
            await unitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);

            if (haveNewCells)
            {
                await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
            }
        }, ct);
    }

    public async Task UpdateColumnAsync(Guid equipmentSheetId, Guid columnId, ColumnModel updatedColumnModel, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columns = DeserializeColumns(columnsJson);
        
        var columnToUpdate = columns.FirstOrDefault(c => c.Id == columnId);
        if (columnToUpdate == null)
        {
            throw new InvalidOperationException($"Column with ID {columnId} not found in equipment sheet {equipmentSheetId}");
        }

        updatedColumnModel.Id = columnId;
        var index = columns.FindIndex(c => c.Id == columnId);
        columns[index] = updatedColumnModel;
        
        var updatedColumnsJson = SerializeColumns(columns);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
        }, ct);
    }

    public async Task UpdateColumnsPositionsAsync(Guid equipmentSheetId, Dictionary<Guid, int> columnNewPositions, CancellationToken ct = default)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
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
            await unitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
        }, ct);
    }

    public async Task UpdateColumnWidthAsync(Guid equipmentSheetId, Guid columnId, double newWidth, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columnsModels = DeserializeColumns(columnsJson);

        var columnToUpdate = columnsModels.FirstOrDefault(c => c.Id == columnId);
        if (columnToUpdate == null)
        {
            throw new InvalidOperationException($"Column with ID {columnId} not found in equipment sheet {equipmentSheetId}");
        }
        
        columnToUpdate.Width = newWidth;
        
        var updatedColumnsJson = SerializeColumns(columnsModels);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
        }, ct);
    }

    public async Task UpdateRowAsync(Guid equipmentSheetId, Guid rowId, RowModel updatedRowModel, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
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
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task UpdateRowsAsync(Guid equipmentSheetId, List<RowModel> rowModels, bool sortByPosition, CancellationToken ct)
    {
        var rowsToSerialize = sortByPosition 
            ? rowModels.OrderBy(r => r.Position).ToList()
            : rowModels;
        
        var updatedRowsJson = SerializeRows(rowsToSerialize);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task UpdateCellValueAsync(Guid equipmentSheetId, Guid cellId, object newValue, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        var rows = DeserializeRows(currentRowsJson);
        
        var rowForUpdate = rows.FirstOrDefault(r => r.Cells.Any(c => c.Id == cellId));
        if (rowForUpdate == null)
        {
            throw new InvalidOperationException($"Row containing cell with ID {cellId} not found in equipment sheet {equipmentSheetId}");
        }
        
        var cellForUpdate = rowForUpdate.Cells.FirstOrDefault(c => c.Id == cellId);
        if (cellForUpdate == null)
        {
            throw new InvalidOperationException($"Cell with ID {cellId} not found in row");
        }
        
        cellForUpdate.Value = newValue;
        
        var updatedRowsJson = SerializeRows(rows);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task SoftRemoveCellsByColumnIdAsync(Guid equipmentSheetId, Guid columnId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columns = DeserializeColumns(columnsJson);
        
        var columnForCellsRemoved = columns.FirstOrDefault(c => c.Id == columnId);
        if (columnForCellsRemoved == null)
        {
            throw new InvalidOperationException($"Column with ID {columnId} not found for equipment sheet {equipmentSheetId}");
        }
        
        var rowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        var rows = DeserializeRows(rowsJson);

        foreach (var cell in rows
                     .Select(rowModel => rowModel.Cells
                         .FirstOrDefault(c => c.ColumnMappingName == columnForCellsRemoved.MappingName))
                     .OfType<CellModel>())
        {
            cell.Deleted = true;
        }
        
        var updatedRowsJson = SerializeRows(rows);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task SoftRemoveRowAsync(Guid equipmentSheetId, Guid rowId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        var rows = DeserializeRows(currentRowsJson);
        
        var rowForUpdate = rows.FirstOrDefault(r => r.Id == rowId);
        if (rowForUpdate == null)
        {
            throw new InvalidOperationException($"Row with ID {rowId} not found for equipment sheet {equipmentSheetId}");
        }

        rowForUpdate.Deleted = true;
        
        var updatedRowsJson = SerializeRows(rows);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task SoftRemoveRowsAsync(Guid equipmentSheetId, List<Guid> rowIds, CancellationToken ct)
    {
       await unitOfWork.EnsureInitializedForReadAsync(ct);
       var currentRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
       var rowModels = DeserializeRows(currentRowsJson);
       
       foreach (var row in rowModels.Where(r => rowIds.Contains(r.Id)))
       {
           row.Deleted = true;
           foreach (var cell in row.Cells)
           {
               cell.Deleted = true;
           }
       }
       
       var updatedRowsJson = SerializeRows(rowModels);
       
       await ExecuteInTransactionAsync(async () =>
       {
           await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
       }, ct);
    }

    public async Task SoftRemoveAllRowsAsync(Guid equipmentSheetId, CancellationToken ct = default)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        
        var allRowsJson = await unitOfWork.EquipmentSheetRepository.GetRowsJsonAsync(equipmentSheetId, ct);
        
        var allRowsModels = DeserializeRows(allRowsJson);
        
        foreach (var row in allRowsModels.Where(row => !row.Deleted))
        {
            row.Deleted = true;
        }
        
        var updatedRowsJson = SerializeRows(allRowsModels);
        
        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateRowsAsync(equipmentSheetId, updatedRowsJson, ct);
        }, ct);
    }

    public async Task SoftRemoveColumnAsync(Guid equipmentSheetId, Guid columnId, CancellationToken ct)
    {
        await unitOfWork.EnsureInitializedForReadAsync(ct);
        var columnsJson = await unitOfWork.EquipmentSheetRepository.GetColumnsJsonAsync(equipmentSheetId, ct);
        var columns = DeserializeColumns(columnsJson);
        
        var columnToSoftRemove = columns.FirstOrDefault(c => c.Id == columnId);
        if (columnToSoftRemove == null)
        {
            throw new InvalidOperationException($"Column with ID {columnId} not found in equipment sheet {equipmentSheetId}");
        }
        
        columnToSoftRemove.SoftDeleted = true;
        
        if (columns.All(c => c.SoftDeleted))
        {
            await SoftRemoveAllRowsAsync(equipmentSheetId, ct);
        }
        
        var updatedColumnsJson = SerializeColumns(columns);

        await ExecuteInTransactionAsync(async () =>
        {
            await unitOfWork.EquipmentSheetRepository.UpdateColumnsAsync(equipmentSheetId, updatedColumnsJson, ct);
        }, ct);
    }

    public Task UpdateColumnPositionAsync(Dictionary<Guid, int> columnPosition, Guid tableId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateColumnWidthAsync(Dictionary<Guid, double> columnWidths, Guid tableId)
    {
        throw new NotImplementedException();
    }
    

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
        await unitOfWork.DisposeAsync();
    }
}