using System.Collections.ObjectModel;
using System.Text;
using Common.Logging;
using Data.AppDbContext;
using Models.EquipmentDataGrid;
using Npgsql;

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

    public async Task<List<string>> GetColumnNamesAsync(string equipmentTableName)
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
}