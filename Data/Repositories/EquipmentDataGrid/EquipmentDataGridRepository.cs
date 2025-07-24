
using Common.Logging;
using Data.ApplicationDbContext;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Data.Repositories.EquipmentDataGrid;

public class EquipmentDataGridRepository : IEquipmentDataGridRepository
{
    private readonly AppDbContext _context;
    private readonly IAppLogger<EquipmentDataGridRepository> _logger;
    public EquipmentDataGridRepository(AppDbContext context, IAppLogger<EquipmentDataGridRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
   public async Task<List<ColumnDto>> GetColumnsAsync(int tableId, CancellationToken cancellationToken)
   {
       var rawData = new List<(int Id, int TableId, string SettingsJson)>();

       await using var connection = await _context.OpenNewConnectionAsync(cancellationToken);
       const string sql = @"SELECT * FROM ""public"".""custom_columns"" 
                            WHERE ""table_id"" = @tableId 
                            AND ""deleted"" = false";
       await using var cmd = new NpgsqlCommand(sql, connection);
       cmd.Parameters.AddWithValue("@tableId", tableId);

       await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
       while (await reader.ReadAsync(cancellationToken))
       {
           rawData.Add((
               reader.GetValueOrDefault<int>("id"),
               reader.GetValueOrDefault<int>("table_id"),
               reader.GetValueOrDefault<string>("settings")
           ));
       }
       
       var result = new List<ColumnDto>(rawData.Count);
       foreach (var (id, tablId, settingsJson) in rawData)
       {
           ColumnSettings settings = null;
           if (!string.IsNullOrWhiteSpace(settingsJson))
           {
               settings = JsonConvert.DeserializeObject<ColumnSettings>(settingsJson);

               if (settings?.SpecificSettings is JObject jObj)
               {
                   settings.SpecificSettings = DeserializeSpecificSettings(settings.DataType, jObj);
               }
           }

           result.Add(new ColumnDto
           {
               Id = id,
               TableId = tablId,
               Settings = settings
           });
       }
       return result;
   }

    public async Task<List<RowItem>> GetRowsAsync(int tableId, CancellationToken cancellationToken)
    {
        await using var connection = await _context.OpenNewConnectionAsync(cancellationToken);
        string sql = @"SELECT
                       r.""id"" as ""row_id"", 
                       json_object_agg(col.""settings"" ->> 'MappingName', c.""value"") AS row_data
                       FROM ""public"".""equipment_rows"" r 
                       JOIN ""public"".""equipment_cells"" c ON r.""id"" = c.""row_id"" 
                       JOIN ""public"".""custom_columns"" col ON c.""column_id"" = col.""id"" 
                       WHERE r.""table_id"" = @tableId
                       AND r.""deleted"" = false
                       AND c.""deleted"" = false
                       AND col.""deleted"" = false
                       GROUP BY r.""id""
                       ORDER BY r.""position""; ";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@tableId", tableId);
        await using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        
        var rawRowData = new List<(int Id, string Json)>();
        while (await reader.ReadAsync(cancellationToken))
        {
            rawRowData.Add((
                reader.GetValueOrDefault<int>("row_id"),
                reader.GetValueOrDefault<string>("row_data")
            ));
        }
        
        var tasks = rawRowData.Select(async x => new RowItem
        {
            Id = x.Id,
            Data = await Task.Run(() => JsonConvert.DeserializeObject<Dictionary<string, object?>>(x.Json), cancellationToken)
        }).ToList();
        
        var result = await Task.WhenAll(tasks);
        
        return new List<RowItem>(result);
    }
    
    public async Task UpdateRowAsync(int rowId, int tableId, Dictionary<string, object?> updatedRowData)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var columns = new Dictionary<string, int>();
            
            const string getColumnsSql = @"SELECT ""id"", ""settings"" ->> 'MappingName' AS ""mapping_name""  FROM ""public"".""custom_columns"" WHERE ""table_id"" = @tableId;";
            var getColumnsCommand = new NpgsqlCommand(getColumnsSql, connection, transaction);
            getColumnsCommand.Parameters.AddWithValue("@tableId", tableId);
            await using var reader = await getColumnsCommand.ExecuteReaderAsync();
            
            while (await reader.ReadAsync())
            {
                var mappingName = reader.GetValueOrDefault<string>("mapping_name");
                var columnId = reader.GetValueOrDefault<int>("id");
                columns[mappingName] = columnId;
            }

            await reader.CloseAsync();

            foreach (var kv in updatedRowData)
            {
                if (!columns.TryGetValue(kv.Key, out var columnId))
                    throw new Exception($"Column with MappingName '{kv.Key}' not found.");
                
                const string checkCellSql = @"SELECT COUNT(*) FROM ""public"".""equipment_cells"" WHERE ""row_id"" = @rowId AND ""column_id"" = @columnId; ";
                
                var checkCellCmd = new NpgsqlCommand(checkCellSql, connection, transaction);
                checkCellCmd.Parameters.AddWithValue("@rowId", rowId);
                checkCellCmd.Parameters.AddWithValue("@columnId", columnId);
                
                var exists = (long)await checkCellCmd.ExecuteScalarAsync() > 0;

                if (exists)
                {
                    const string updateCellSql = @"UPDATE ""public"".""equipment_cells"" SET ""value"" = @value WHERE ""row_id"" = @rowId AND ""column_id"" = @columnId; ";
                    var updateCellCmd = new NpgsqlCommand(updateCellSql, connection, transaction);
                    updateCellCmd.Parameters.AddWithValue("@rowId", rowId);
                    updateCellCmd.Parameters.AddWithValue("@columnId", columnId);
                    updateCellCmd.Parameters.AddWithValue("@value", kv.Value?.ToString() ?? string.Empty);
                    await updateCellCmd.ExecuteNonQueryAsync();
                }
                else
                {
                    const string insertCellSql = @"INSERT INTO ""public"".""equipment_cells"" (""row_id"", ""column_id"", ""value"") VALUES (@rowId, @columnId, @value); ";
                    var insertCellCmd = new NpgsqlCommand(insertCellSql, connection, transaction);
                    insertCellCmd.Parameters.AddWithValue("@rowId", rowId);
                    insertCellCmd.Parameters.AddWithValue("@columnId", columnId);
                    insertCellCmd.Parameters.AddWithValue("@value", kv.Value?.ToString() ?? string.Empty);
                    await insertCellCmd.ExecuteNonQueryAsync();
                }
            }
            await transaction.CommitAsync();
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Error updating rows");
            throw;
        }
    }

    public async Task UpdateColumnAsync(ColumnDto column)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = @"UPDATE ""public"".""custom_columns"" SET ""settings"" = @settings WHERE ""id"" = @id";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@id", column.Id);
            var settingsJson = JsonConvert.SerializeObject(column.Settings);
            cmd.Parameters.AddWithValue("@settings", NpgsqlDbType.Jsonb, settingsJson);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error updating column");
            throw;
        }
    }

    public async Task UpdateColumnPositionAsync(Dictionary<int, int> columnPositions, int tableId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            string sql = @"UPDATE ""public"".""custom_columns""
                       SET ""settings"" = jsonb_set(""settings"", '{ColumnPosition}', to_jsonb(@columnPosition), true) 
                       WHERE ""id"" = @columnId AND ""table_id"" = @tableId; ";

            foreach (var kvp in columnPositions)
            {
                int columnId = kvp.Key;
                int newPosition = kvp.Value;
                
                Console.WriteLine($"COLUMN ID: {columnId}, NEW POSITION: {newPosition}");
            
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@columnId", columnId);
                cmd.Parameters.AddWithValue("@tableId", tableId);
                cmd.Parameters.AddWithValue("@columnPosition", newPosition);
                await cmd.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error updating column position");
            throw;
        }
    }

    public async Task UpdateColumnWidthAsync(Dictionary<int, double> columnWidths, int tableId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            string sql = @"UPDATE ""public"".""custom_columns""
                       SET ""settings"" = jsonb_set(""settings"", '{ColumnWidth}', to_jsonb(@columnWidth), true) 
                       WHERE ""id"" = @columnId AND ""table_id"" = @tableId; ";

            foreach (var kvp in columnWidths)
            {
                int columnId = kvp.Key;
                double newWidth = kvp.Value;
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@columnId", columnId);
                cmd.Parameters.AddWithValue("@tableId", tableId);
                cmd.Parameters.AddWithValue("@columnWidth", newWidth);
                await cmd.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error updating column width");
            throw;
        }
    }

    public async Task<int> CreateColumnAsync(ColumnSettings column, int tableId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            string sql = @"INSERT INTO ""public"".""custom_columns"" (""table_id"", ""settings"") VALUES (@tableId, @settings::jsonb) RETURNING id;";
            
            string settingsJson = JsonConvert.SerializeObject(column);
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("tableId", tableId);
            cmd.Parameters.AddWithValue("settings", settingsJson);
            
            var result = await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();
            
            return Convert.ToInt32(result);
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error creating column");
            throw;
        }
    }

    public async Task<int> InsertRowAsync(int tableId, int position, Dictionary<string, object?> newRowData, CancellationToken ct)
    {
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            const string insertRowSql = @"INSERT INTO ""public"".""equipment_rows"" (""table_id"", ""position"") VALUES (@tableId, @position) RETURNING id;";
            var insertCommand = new NpgsqlCommand(insertRowSql, connection, transaction);
            insertCommand.Parameters.AddWithValue("@tableId", tableId);
            insertCommand.Parameters.AddWithValue("@position", position);
            var rowId = (int) await insertCommand.ExecuteScalarAsync();

            var columns = new Dictionary<string, int>();
            
            const string getColumnsSql = @"SELECT ""id"", ""settings"" ->> 'MappingName' AS ""mapping_name""  FROM ""public"".""custom_columns"" WHERE ""table_id"" = @tableId;";
            var getColumnsCommand = new NpgsqlCommand(getColumnsSql, connection, transaction);
            getColumnsCommand.Parameters.AddWithValue("@tableId", tableId);
            await using var reader = await getColumnsCommand.ExecuteReaderAsync(ct);
            
            while (await reader.ReadAsync(ct))
            {
                var mappingName = reader.GetValueOrDefault<string>("mapping_name");
                var columnId = reader.GetValueOrDefault<int>("id");
                columns[mappingName] = columnId;
            }

            await reader.CloseAsync();

            foreach (var kv in newRowData)
            {
                if (!columns.TryGetValue(kv.Key, out var columnId))
                    throw new Exception($"Column with MappingName '{kv.Key}' not found.");
                
                var insertCellSql = @"INSERT INTO ""public"".""equipment_cells"" (""row_id"", ""column_id"", ""value"") VALUES (@rowId, @columnId, @value);";
                var insertCellCommand = new NpgsqlCommand(insertCellSql, connection, transaction);
                insertCellCommand.Parameters.AddWithValue("@rowId", rowId);
                insertCellCommand.Parameters.AddWithValue("@columnId", columnId);
                insertCellCommand.Parameters.AddWithValue("@value", kv.Value?.ToString() ?? string.Empty);
                await insertCellCommand.ExecuteNonQueryAsync(ct);
            }
            await transaction.CommitAsync(ct);
            
            return rowId;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            _logger.LogError(e, "Database error creating equipment");
            throw;
        }
    }

    public async Task<List<int>> InsertRowsAsync(int tableId, List<(int position, Dictionary<string, object?> data)> rows, CancellationToken ct)
    {
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            const string insertRowsAsync = @"INSERT INTO ""public"".""equipment_rows"" (""table_id"", ""position"") VALUES (@tableId, @position) RETURNING ""id"";";
            var insertRowsCommand = new NpgsqlCommand(insertRowsAsync, connection, transaction);
            var newRowsIds = new List<int>();
            foreach (var row in rows)
            {
                insertRowsCommand.Parameters.AddWithValue("@tableId", tableId);
                insertRowsCommand.Parameters.AddWithValue("@position", row.position);
                var rowId = (int)await insertRowsCommand.ExecuteScalarAsync(ct);
                newRowsIds.Add(rowId);
            }

            const string getColumnIds = @"SELECT ""id"", ""settings"" ->> 'MappingName' AS ""mapping_name"" FROM ""public"".""custom_columns"" WHERE ""settings"" ->> 'MappingName' = ANY(@mappingNames);";
            var getColumnsCommand = new NpgsqlCommand(getColumnIds, connection, transaction);
            var mappingNames = rows.SelectMany(r => r.data.Select(d => d.Key)).ToList();
            getColumnsCommand.Parameters.AddWithValue("@mappingNames", mappingNames);
            var columnMap = new Dictionary<string, int>();
            await using var reader = await getColumnsCommand.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                var id = reader.GetValueOrDefault<int>("id");
                var mappingName = reader.GetValueOrDefault<string>("mapping_name");
                columnMap[mappingName] = id;
            }
            await reader.CloseAsync();

            const string insertCellsAsync = @"INSERT INTO ""public"".""equipment_cells"" (""row_id"", ""column_id"", ""value"") VALUES (@rowId, @columnId, @value)";
            var insertCellsCommand = new NpgsqlCommand(insertCellsAsync, connection, transaction);
            
            for (int i = 0; i < rows.Count; i++)
            {
                var rowId = newRowsIds[i];
                var rowData = rows[i].data;

                foreach (var kvp in rowData)
                {
                    var mappingName = kvp.Key;
                    var value = kvp.Value;
                    columnMap.TryGetValue(mappingName, out var columnId);
                    
                    insertCellsCommand.Parameters.Clear();
                    insertCellsCommand.Parameters.AddWithValue("@rowId", rowId);
                    insertCellsCommand.Parameters.AddWithValue("@columnId", columnId);
                    insertCellsCommand.Parameters.AddWithValue("@value", value ?? DBNull.Value);
                    await insertCellsCommand.ExecuteNonQueryAsync(ct);
                }
            }
            await transaction.CommitAsync(ct);
            return newRowsIds;
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RemoveItemsAsync(List<int> rowsIds, CancellationToken ct)
    {
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            const string removeCellsSql = @"UPDATE ""public"".""equipment_cells"" 
                                                   SET ""deleted"" = true
                                                   WHERE ""row_id"" = ANY(@rowsIds)";
            await using var removeCellsCmd = new NpgsqlCommand(removeCellsSql, connection, transaction);
            removeCellsCmd.Parameters.AddWithValue("@rowsIds", rowsIds);
            await removeCellsCmd.ExecuteNonQueryAsync(ct);

            const string removeRowsSql = @"UPDATE ""public"".""equipment_rows""
                                           SET ""deleted"" = true 
                                           WHERE ""id"" = ANY(@rowsIds)";
            
            await using var removeRowsCmd = new NpgsqlCommand(removeRowsSql, connection, transaction);
            removeRowsCmd.Parameters.AddWithValue("@rowsIds", rowsIds);
            await removeRowsCmd.ExecuteNonQueryAsync(ct);
            
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task RemoveColumnAsync(int columnId, CancellationToken ct)
    {
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);

        try
        {
            const string removeCellsSql = @"UPDATE ""public"".""equipment_cells"" 
                                            SET ""deleted"" = true
                                            WHERE ""column_id"" = @columnId";
            await using var removeCellsCmd = new NpgsqlCommand(removeCellsSql, connection, transaction);
            removeCellsCmd.Parameters.AddWithValue("@columnId", columnId);
            await removeCellsCmd.ExecuteNonQueryAsync(ct);

            const string removeColumnSql = @"UPDATE ""public"".""custom_columns"" 
                                             SET ""deleted"" = true 
                                             WHERE ""id"" = @columnId";
            await using var removeColumnCmd = new NpgsqlCommand(removeColumnSql, connection, transaction);
            removeColumnCmd.Parameters.AddWithValue("@columnId", columnId);
            await removeColumnCmd.ExecuteNonQueryAsync(ct);
            
            await transaction.CommitAsync(ct);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
    
    private static object? DeserializeSpecificSettings(ColumnDataType dataType, JObject jObject)
    {
        return dataType switch
        {
            ColumnDataType.Text => jObject.ToObject<TextColumnSettings>(),
            ColumnDataType.Number => jObject.ToObject<NumberColumnSettings>(),
            ColumnDataType.Boolean => jObject.ToObject<BooleanColumnSettings>(),
            ColumnDataType.Date => jObject.ToObject<DateColumnSettings>(),
            ColumnDataType.Currency => jObject.ToObject<CurrencyColumnSettings>(),
            ColumnDataType.List => jObject.ToObject<ListColumnSettings>(),
            ColumnDataType.MultilineText => jObject.ToObject<MultilineTextColumnSettings>(),
            _ => null
        };
    }
}