using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Documents;
using Common.Logging;
using Data.ApplicationDbContext;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Npgsql;
using NpgsqlTypes;

namespace Data.Repositories.Repairs;

public class AddRepairRepository : IAddRepairRepository
{
    private readonly AppDbContext _context;
    private readonly IAppLogger<AddRepairRepository> _logger;

    public AddRepairRepository(AppDbContext context, IAppLogger<AddRepairRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<EquipmentDto>> GetDataAsync(string equipmentTableName)
    {
        var equipments = new List<EquipmentDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        try
        {
            string sql = $@"SELECT ""id"", ""Інвентарний номер"", ""Бренд"", ""Модель"" FROM ""UserTables"".""{equipmentTableName}"" WHERE ""IsWriteOff"" = false AND ""CopyOfData"" = false; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    equipments.Add(new EquipmentDto
                    {
                        EquipmentId = reader.GetInt32(0),
                        EquipmentInventoryNumber = reader.IsDBNull(1) ? null : reader.GetString(1),
                        EquipmentBrand = reader.IsDBNull(2) ? null : reader.GetString(2),
                        EquipmentModel = reader.IsDBNull(3) ? null : reader.GetString(3),
                    });
                }
            }
            return equipments;
        }
        catch (NpgsqlException e)
        {
            _logger.LogError(e, "Database error getting equipments");
            throw;
        }
    }

    public async Task<int> SaveRepairAsync(RepairData repairData, string repairsTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql =
                $"INSERT INTO \"UserTables\".\"{repairsTableName}\" (\"Об'єкт\", \"Опис поломки\", \"Дата початку\", \"Дата кінця\", \"Витрачено часу\", \"Працівник\", \"Статус\") " +
                "VALUES (@object, @breakDescription, @dateStart, @dateEnd, @spentTime, @worker, @status) RETURNING id; ";
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@object", repairData.EquipmentId);
            cmd.Parameters.AddWithValue("@breakDescription", NpgsqlDbType.Text).Value = repairData.BreakDescription ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@dateStart", NpgsqlDbType.Date).Value = repairData.StartRepair ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@dateEnd", NpgsqlDbType.Date).Value = repairData.EndRepair ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@spentTime", NpgsqlDbType.Timestamp).Value = repairData.TimeSpentOnRepair ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@worker", repairData.Worker);
            cmd.Parameters.AddWithValue("@status", repairData.RepairStatus);
            var result = await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();
            int id = Convert.ToInt32(result);
            return id;
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error save repair");
            throw;
        }
    }

public async Task InsertUsedMaterialsAsync(
    ObservableCollection<RepairConsumableDto> repairConsumableDtos,
    int repairId,
    string repairConsumablesTableName)
{
    await using var connection = await _context.OpenNewConnectionAsync();
    await using var transaction = await connection.BeginTransactionAsync();

    try
    {
        _logger.LogInformation("Inserting in database used materials for repair consumable");
        var sqlInsertUsedMaterials = new StringBuilder($@"INSERT INTO ""UserTables"".""{repairConsumablesTableName}"" 
            (""Робота"", ""Категорія"", ""Назва"", ""Одиниця"", ""Витрачено"") VALUES ");
        var parameters = new List<NpgsqlParameter>();

        for (int i = 0; i < repairConsumableDtos.Count; i++)
        {
            if (i > 0) sqlInsertUsedMaterials.Append(", ");
            sqlInsertUsedMaterials.Append($"(@repairId, @category{i}, @name{i}, @unit{i}, @quantity{i})");
            parameters.Add(new NpgsqlParameter($"@category{i}", repairConsumableDtos[i].Category));
            parameters.Add(new NpgsqlParameter($"@name{i}", repairConsumableDtos[i].Name));
            parameters.Add(new NpgsqlParameter($"@unit{i}", repairConsumableDtos[i].Unit));
            parameters.Add(new NpgsqlParameter($"@quantity{i}", repairConsumableDtos[i].SpentMaterial));
        }
        parameters.Add(new NpgsqlParameter("@repairId", repairId));

        await using (var cmdInsertUsedMaterials = new NpgsqlCommand(sqlInsertUsedMaterials.ToString(), connection, transaction))
        {
            cmdInsertUsedMaterials.Parameters.AddRange(parameters.ToArray());
            await cmdInsertUsedMaterials.ExecuteNonQueryAsync();
        }

        var groupedByOperationsAndMaterial = repairConsumableDtos
    .GroupBy(x => new { x.OperationsConsumableTableName, x.MaterialId });

        foreach (var group in groupedByOperationsAndMaterial)
        {
            string operationsTableName = group.Key.OperationsConsumableTableName;
            int materialId = group.Key.MaterialId;
            string materialsTableName = group.First().ConsumableTableName;
            
            decimal? currentBalance;
            var sqlGetBalance = $@"SELECT ""Залишок"" FROM ""ConsumablesSchema"".""{materialsTableName}"" WHERE ""id"" = @materialId;";
            await using (var cmdGetBalance = new NpgsqlCommand(sqlGetBalance, connection, transaction))
            {
                cmdGetBalance.Parameters.AddWithValue("@materialId", materialId);
                var result = await cmdGetBalance.ExecuteScalarAsync();
                currentBalance = result == null || result == DBNull.Value ? 0m : Convert.ToDecimal(result);
            }
            
            foreach (var item in repairConsumableDtos.Where(x =>
                         x.OperationsConsumableTableName == operationsTableName && x.MaterialId == materialId))
            {
                currentBalance -= item.SpentMaterial;

                var sqlAddOperation = $@"INSERT INTO ""ConsumablesSchema"".""{operationsTableName}"" 
                    (""Матеріал"", ""Кількість"", ""Залишок після"", ""Тип операції"", ""Дата, час"") 
                    VALUES (@materialId, @spentMaterial, @balanceAfter, @operationType, @dateTime);";

                await using var cmdAddOperation = new NpgsqlCommand(sqlAddOperation, connection, transaction);
                cmdAddOperation.Parameters.AddWithValue("@materialId", materialId);
                cmdAddOperation.Parameters.AddWithValue("@spentMaterial", item.SpentMaterial);
                cmdAddOperation.Parameters.AddWithValue("@balanceAfter", currentBalance);
                cmdAddOperation.Parameters.AddWithValue("@operationType", "Списання");
                cmdAddOperation.Parameters.AddWithValue("@dateTime", DateTime.Now);
                await cmdAddOperation.ExecuteNonQueryAsync();
            }
        }
        
        var groupedConsumablesTableName = repairConsumableDtos.GroupBy(x => x.ConsumableTableName);
        foreach (var group in groupedConsumablesTableName)
        {
            string tableName = group.Key;
            var items = group.ToList();

            foreach (var item in items)
            {
                var sqlUpdateQuantity = $@"UPDATE ""ConsumablesSchema"".""{tableName}"" 
                                           SET ""Залишок"" = ""Залишок"" - @spentMaterial 
                                           WHERE ""id"" = @materialId;";
                await using var cmdUpdateQuantity = new NpgsqlCommand(sqlUpdateQuantity, connection, transaction);
                cmdUpdateQuantity.Parameters.AddWithValue("@spentMaterial", item.SpentMaterial);
                cmdUpdateQuantity.Parameters.AddWithValue("@materialId", item.MaterialId);
                await cmdUpdateQuantity.ExecuteNonQueryAsync();
            }
        }

        await transaction.CommitAsync();
        _logger.LogInformation("Successfully inserted used materials for repair consumable");
    }
    catch (NpgsqlException e)
    {
        await transaction.RollbackAsync();
        _logger.LogError(e, "Database error inserting used materials");
        throw;
    }
}

    public async Task<List<RepairConsumableDto>> GetUsedMaterialsAsync(string repairConsumablesTableName, int repairId)
    {
        var usedMaterials = new List<RepairConsumableDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        try
        {
            string sql = $@"SELECT * FROM ""UserTables"".""{repairConsumablesTableName}"" WHERE ""Робота"" = @repairId; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@repairId", repairId);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    usedMaterials.Add(new RepairConsumableDto
                    {
                        Category = reader.GetValueOrDefault<string>("Категорія"),
                        Name = reader.GetValueOrDefault<string>("Назва"),
                        Unit = reader.GetValueOrDefault<string>("Одиниця"),
                        SpentMaterial = reader.GetValueOrDefault<decimal?>("Витрачено")
                    });
                }
            }
            return usedMaterials;
        }
        catch (NpgsqlException e)
        {
            _logger.LogError(e, "Failed to get used materials for repair");
            throw;
        }
    }

    public async Task UpdateRepairAsync(RepairData repairData, string repairsTableName, int repairId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"UPDATE ""UserTables"".""{repairsTableName}"" 
                                                            SET 
                                                            ""Опис поломки"" = @description, 
                                                            ""Дата початку"" = @dateTimeStart,
                                                            ""Дата кінця"" = @dateTimeEnd,
                                                            ""Витрачено часу"" = @spentTime,
                                                            ""Працівник"" = @worker,
                                                            ""Статус"" = @repairStatus 
                                                            WHERE ""id"" = @repairId; ";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@repairId", repairId);
            cmd.Parameters.AddWithNullableValue("@dateTimeStart", repairData.StartRepair, NpgsqlDbType.Timestamp);
            cmd.Parameters.AddWithNullableValue("@dateTimeEnd", repairData.EndRepair, NpgsqlDbType.Timestamp);
            cmd.Parameters.AddWithNullableValue("@spentTime", repairData.TimeSpentOnRepair, NpgsqlDbType.Interval);
            cmd.Parameters.AddWithNullableValue("@description", repairData.BreakDescription, NpgsqlDbType.Text);
            cmd.Parameters.AddWithValue("@worker", repairData.Worker);
            cmd.Parameters.AddWithValue("@repairStatus", repairData.RepairStatus);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database failed to update repair");
            throw;
        }
    }
}