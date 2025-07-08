using System.Text;
using Common.Logging;
using Data.AppDbContext;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Data.Repositories.EquipmentDataGrid;

public class EquipmentDataGridRepository : IEquipmentDataGridRepository
{
    private readonly DbContext _context;
    private readonly IAppLogger<EquipmentDataGridRepository> _logger;
    public EquipmentDataGridRepository(DbContext context, IAppLogger<EquipmentDataGridRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

   /* public async Task<List<string>> GetColumnNamesAsync(string equipmentTableName)
    {
        var columnNames = new List<string>();
        await using var connection = await _context.OpenNewConnectionAsync();
        
        string sql = $@"SELECT column_name FROM information_schema.columns WHERE 
                                                        table_schema = 'UserTables' AND 
                                                        table_name = '{equipmentTableName}'; ";
            
        await using var cmd = new NpgsqlCommand(sql, connection);
        await using var reader = await cmd.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            columnNames.Add(reader.GetString(0));
        }
            
        return columnNames;
    }

    public async Task<List<EquipmentDto>> GetEquipmentListAsync(string equipmentTableName)
    {
        var equipmentList = new List<EquipmentDto>();

    using var connection = await _context.OpenNewConnectionAsync();
    string sql = $@"SELECT * FROM ""UserTables"".""{equipmentTableName}"" WHERE ""IsWriteOff"" = false AND ""CopyOfData"" = false;";
    
    await using var cmd = new NpgsqlCommand(sql, connection);
    await using var reader = await cmd.ExecuteReaderAsync();
    
    var availableColumns = Enumerable.Range(0, reader.FieldCount)
                                     .Select(reader.GetName)
                                     .ToHashSet();

    while (await reader.ReadAsync())
    {
        var dto = new EquipmentDto
        {
            Id = reader.GetValueOrDefault<int>("id"),
            InventoryNumber = TryGet<string>(reader, availableColumns, "Інвентарний номер"),
            Brand = TryGet<string>(reader, availableColumns, "Бренд"),
            Model = TryGet<string>(reader, availableColumns, "Модель"),
            Category = TryGet<string>(reader, availableColumns, "Категорія"),
            SerialNumber = TryGet<string>(reader, availableColumns, "Серійний номер"),
            Class = TryGet<string>(reader, availableColumns, "Клас"),
            Year = TryGet<int>(reader, availableColumns, "Рік"),
            Height = TryGet<decimal>(reader, availableColumns, "Висота (см)"),
            Width = TryGet<decimal>(reader, availableColumns, "Ширина (см)"),
            Length = TryGet<decimal>(reader, availableColumns, "Довжина (см)"),
            Weight = TryGet<decimal>(reader, availableColumns, "Вага (кг)"),
            Floor = TryGet<string>(reader, availableColumns, "Поверх"),
            Department = TryGet<string>(reader, availableColumns, "Відділ"),
            Room = TryGet<string>(reader, availableColumns, "Кімната"),
            Consumption = TryGet<decimal>(reader, availableColumns, "Споживання (кв/год)"),
            Voltage = TryGet<decimal>(reader, availableColumns, "Напруга (В)"),
            Water = TryGet<decimal>(reader, availableColumns, "Вода (л/год)"),
            Air = TryGet<decimal>(reader, availableColumns, "Повітря (л/год)"),
            BalanceCost = TryGet<decimal>(reader, availableColumns, "Балансова вартість (грн)"),
            Notes = TryGet<string>(reader, availableColumns, "Нотатки"),
            ResponsiblePerson = TryGet<string>(reader, availableColumns, "Відповідальний"),
        };

        equipmentList.Add(dto);
    }

    return equipmentList;
    }
    
    private static T? TryGet<T>(NpgsqlDataReader reader, HashSet<string> columns, string columnName)
    {
        return columns.Contains(columnName) ? reader.GetValueOrDefault<T>(columnName) : default;
    }

    public async Task<int> InsertEquipmentAsync(EquipmentDto equipment, string equipmentTableName)
    {
        var columns = await GetColumnNamesAsync(equipmentTableName);
        
        var columnNames = new List<string>();
        var paramNames = new List<string>();
        var parameters = new List<NpgsqlParameter>();
        
        foreach (var column in columns)
        {
            if (column.ToLower() == "id") 
                continue; 
            
            object? value = column.ToLower() switch
            {
                "інвентарний номер" => equipment.InventoryNumber,
                "бренд" => equipment.Brand,
                "модель" => equipment.Model,
                "категорія" => equipment.Category,
                "серійний номер" => equipment.SerialNumber,
                "клас" => equipment.Class,
                "рік" => equipment.Year,
                "висота (см)" => equipment.Height,
                "ширина (см)" => equipment.Width,
                "довжина (см)" => equipment.Length,
                "вага (кг)" => equipment.Weight,
                "поверх" => equipment.Floor,
                "відділ" => equipment.Department,
                "кімната" => equipment.Room,
                "споживання (кв/год)" => equipment.Consumption,
                "напруга (в) " => equipment.Voltage,
                "вода (л/год)" => equipment.Water,
                "повітря (л/год)" => equipment.Air,
                "балансова вартість (грн)" => equipment.BalanceCost,
                "нотатки" => equipment.Notes,
                "відповідальний" => equipment.ResponsiblePerson,
                "iswriteoff" => false,
                "copyofdata" => false,
                _ => DBNull.Value,
            };
            columnNames.Add($@"""{column}""");
            var paramName = "@" + column.Replace(" ", "_").Replace("(", "").Replace(")", "").Replace("/", "_");
            paramNames.Add(paramName);
            parameters.Add(new NpgsqlParameter(paramName, value ?? DBNull.Value));
        }

        var sql = new StringBuilder();
        sql.Append($@"INSERT INTO ""UserTables"".""{equipmentTableName}"" (");
        sql.Append(string.Join(", ", columnNames));
        sql.Append(") VALUES (");
        sql.Append(string.Join(", ", paramNames));
        sql.Append(@") RETURNING ""id"";");
        
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            await using var cmd = new NpgsqlCommand(sql.ToString(), connection, transaction);
            cmd.Parameters.AddRange(parameters.ToArray());
            var newId= (int)await cmd.ExecuteScalarAsync();
            transaction.Commit();
            return newId;
        }
        catch (Exception e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error inserting equipment");
            throw;
        }
    }

    public async Task UpdateEquipmentAsync(EquipmentDto equipment, string equipmentTableName)
    {
        var columns = await GetColumnNamesAsync(equipmentTableName);

        var columnNames = new List<string>();
        var paramNames   = new List<string>();
        var parameters   = new List<NpgsqlParameter>();


        foreach (var column in columns)
        {
            if (column.ToLower() == "id") 
                continue;

            object? value = column.ToLower() switch
            {
                "інвентарний номер" => equipment.InventoryNumber,
                "бренд" => equipment.Brand,
                "модель" => equipment.Model,
                "категорія" => equipment.Category,
                "серійний номер" => equipment.SerialNumber,
                "клас" => equipment.Class,
                "рік" => equipment.Year,
                "висота (см)" => equipment.Height,
                "ширина (см)" => equipment.Width,
                "довжина (см)" => equipment.Length,
                "вага (кг)" => equipment.Weight,
                "поверх" => equipment.Floor,
                "відділ" => equipment.Department,
                "кімната" => equipment.Room,
                "споживання (кв/год)" => equipment.Consumption,
                "напруга (в)" => equipment.Voltage,
                "вода (л/год)" => equipment.Water,
                "повітря (л/год)" => equipment.Air,
                "балансова вартість (грн)" => equipment.BalanceCost,
                "нотатки" => equipment.Notes,
                "відповідальний" => equipment.ResponsiblePerson,
                "iswriteoff" => false,
                "copyofdata" => false,
                _ => DBNull.Value,
            };
            
            columnNames.Add($@"""{column}""");
            
            var safeName = column
                .Replace(" ", "_")
                .Replace("(", "")
                .Replace(")", "")
                .Replace("/", "_");
            var paramName = "@" + safeName;
            
            paramNames.Add(paramName);
            parameters.Add(new NpgsqlParameter(paramName, value ?? DBNull.Value));
        }
        parameters.Add(new NpgsqlParameter("@id", equipment.Id));
        
        var setClauses = columnNames
            .Select((col, idx) => $"{col} = {paramNames[idx]}")
            .ToList();

        var sql = new StringBuilder();
        sql.Append($@"UPDATE ""UserTables"".""{equipmentTableName}"" SET ");
        sql.Append(string.Join(", ", setClauses));
        sql.Append(@" WHERE ""id"" = @id");

        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();

        try
        {
            await using var cmd = new NpgsqlCommand(sql.ToString(), connection, transaction);
            cmd.Parameters.AddRange(parameters.ToArray());

            await cmd.ExecuteNonQueryAsync();
            transaction.Commit();
            

        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error updating equipment");
            throw;
        }
    }

    public async Task WriteOffEquipmentAsync(int equipmentId, string equipmentTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"UPDATE ""UserTables"".""{equipmentTableName}"" SET ""IsWriteOff"" = true WHERE ""id"" = @id";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@id", equipmentId);
            await cmd.ExecuteNonQueryAsync();
            transaction.Commit();
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error writeoff equipment");
            throw;
        }
    }

    public async Task MakeDataCopyAsync(int equipmentId, string equipmentTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"UPDATE ""UserTables"".""{equipmentTableName}"" SET ""CopyOfData"" = true WHERE ""id"" = @id";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.Add(new NpgsqlParameter("@id", equipmentId));
            await cmd.ExecuteNonQueryAsync();
            transaction.Commit();
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error while make copy of equipment data");
            throw;
        }
    }

    public async Task<List<SparePartDto>> GetSparePartListAsync(int equipmentId, string sparePartTableName)
    {
        var sparePartsFromDb = new List<SparePartDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        string sql = $@"SELECT * FROM ""UserTables"".""{sparePartTableName}"" WHERE ""EquipmentId"" = @equipmentId; ";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.Add(new NpgsqlParameter("@equipmentId", equipmentId));
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            sparePartsFromDb.Add(new SparePartDto
            {
                Id = reader.GetValueOrDefault<int>("id"),
                EquipmentId = reader.GetValueOrDefault<int>("EquipmentId"),
                SparePartName = reader.GetValueOrDefault<string>("Назва"),
                SparePartCategory = reader.GetValueOrDefault<string>("Категорія"),
                SparePartQuantity = reader.GetValueOrDefault<decimal>("Кількість"),
                SparePartUnit = reader.GetValueOrDefault<string>("Одиниця"),
                SparePartSerialNumber = reader.GetValueOrDefault<string>("Серійний номер"),
                SparePartNotes = reader.GetValueOrDefault<string>("Примітки")
            });
        }
        return sparePartsFromDb;
    }

    public async Task<int> InsertSparePartAsync(SparePartDto sparePart, string sparePartTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"INSERT INTO ""UserTables"".""{sparePartTableName}"" (""EquipmentId"", ""Назва"", ""Кількість"", ""Одиниця"", ""Серійний номер"", ""Примітки"", ""Категорія"") 
                        VALUES (@equipmentId, @name, @quantity, @unit, @serialNumber, @notes, @category) RETURNING ""id""; ";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@equipmentId", sparePart.EquipmentId);
            cmd.Parameters.AddWithValue("@name", sparePart.SparePartName);
            cmd.Parameters.AddWithNullableValue("@quantity", (decimal?)sparePart.SparePartQuantity, NpgsqlDbType.Numeric);
            cmd.Parameters.AddWithValue("@unit", sparePart.SparePartUnit);
            cmd.Parameters.AddWithNullableValue("@serialNumber", sparePart.SparePartSerialNumber, NpgsqlDbType.Text);
            cmd.Parameters.AddWithNullableValue("@notes", sparePart.SparePartNotes, NpgsqlDbType.Text);
            cmd.Parameters.AddWithNullableValue("@category", sparePart.SparePartCategory, NpgsqlDbType.Text);
            var newId = (int)await cmd.ExecuteScalarAsync();
            transaction.Commit();
            return newId;
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error inserting spare part");
            throw;
        }
    }

    public async Task UpdateSparePartAsync(SparePartDto sparePart, string sparePartTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"UPDATE ""UserTables"".""{sparePartTableName}"" SET 
                                                            ""Назва"" = @name,
                                                            ""Кількість"" = @quantity,
                                                            ""Одиниця"" = @unit,
                                                            ""Серійний номер"" = @serialNumber,
                                                            ""Примітки"" = @notes,
                                                            ""Категорія"" = @category 
                                                            WHERE ""id"" = @id; ";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@id", sparePart.Id);
            cmd.Parameters.AddWithValue("@name", sparePart.SparePartName);
            cmd.Parameters.AddWithNullableValue("@quantity", (decimal?)sparePart.SparePartQuantity, NpgsqlDbType.Numeric);
            cmd.Parameters.AddWithValue("@unit", sparePart.SparePartUnit);
            cmd.Parameters.AddWithNullableValue("@serialNumber", sparePart.SparePartSerialNumber, NpgsqlDbType.Text);
            cmd.Parameters.AddWithNullableValue("@notes", sparePart.SparePartNotes, NpgsqlDbType.Text);
            cmd.Parameters.AddWithNullableValue("@category", sparePart.SparePartCategory, NpgsqlDbType.Text);
            await cmd.ExecuteNonQueryAsync();
            transaction.Commit();
        }
        catch (NpgsqlException e)
        {
            transaction.Rollback();
            _logger.LogError(e, "Database error updating spare part");
            throw;
        }
    }*/
    
   public async Task<List<ColumnDto>> GetColumnsAsync(int tableId)
   {
       var rawData = new List<(int Id, int TableId, string SettingsJson)>();

       await using var connection = await _context.OpenNewConnectionAsync();
       const string sql = @"SELECT * FROM ""public"".""custom_columns"" WHERE ""table_id"" = @tableId";
       await using var cmd = new NpgsqlCommand(sql, connection);
       cmd.Parameters.AddWithValue("@tableId", tableId);

       await using var reader = await cmd.ExecuteReaderAsync();
       while (await reader.ReadAsync())
       {
           rawData.Add((
               reader.GetValueOrDefault<int>("id"),
               reader.GetValueOrDefault<int>("table_id"),
               reader.GetValueOrDefault<string>("settings")
           ));
       }
       
       return await Task.Run(() =>
       {
           var result = new List<ColumnDto>(rawData.Count);
           foreach (var (id, tableId, settingsJson) in rawData)
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
                   TableId = tableId,
                   Settings = settings
               });
           }

           return result;
       });
   }

    public async Task<List<EquipmentDto>> GetRowsAsync(int tableId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        string sql = @"SELECT * FROM ""public"".""custom_data"" WHERE ""table_id"" = @tableId";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@tableId", tableId);
        await using var reader = await cmd.ExecuteReaderAsync();

        var rawData = new List<(int id, int tableId, int rowIndex, string json)>();
        while (await reader.ReadAsync())
        {
            rawData.Add((
                reader.GetValueOrDefault<int>("id"),
                reader.GetValueOrDefault<int>("table_id"),
                reader.GetValueOrDefault<int>("row_index"),
                reader.GetValueOrDefault<string>("data")
            ));
        }
        
        return await Task.Run(() =>
        {
            return rawData.Select(row => new EquipmentDto
            {
                Id = row.id,
                TableId = row.tableId,
                RowIndex = row.rowIndex,
                Data = string.IsNullOrEmpty(row.json)
                    ? new Dictionary<string, object>()
                    : JsonConvert.DeserializeObject<Dictionary<string, object>>(row.json)
            }).ToList();
        });
    }

    public async Task UpdateRowsAsync(IDictionary<string, object> rows, int id)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            string sql = @"UPDATE ""public"".""custom_data"" SET ""data"" = @data WHERE ""id"" = @id";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@id", id);
            var dataJson = JsonConvert.SerializeObject(rows);
            cmd.Parameters.AddWithValue("@data", NpgsqlDbType.Jsonb, dataJson);
            await cmd.ExecuteNonQueryAsync();
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

    private object? DeserializeSpecificSettings(ColumnDataType dataType, JObject jObject)
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

    public async Task<int> AddNewRowAsync(EquipmentDto equipment)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            string sql = @"INSERT INTO ""public"".""custom_data"" (""table_id"", ""data"", ""row_index"") VALUES (@tableId, @data::jsonb, @rowIndex) RETURNING id;";
            string dataJson = JsonConvert.SerializeObject(equipment.Data);
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("tableId", equipment.TableId);
            cmd.Parameters.AddWithValue("data", dataJson);
            cmd.Parameters.AddWithValue("rowIndex", equipment.RowIndex);
            var result = await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();   
            return Convert.ToInt32(result);
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error creating equipment");
            throw;
        }
    }
}