using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Documents;
using Common.Logging;
using Data.AppDbContext;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Npgsql;
using NpgsqlTypes;

namespace Data.Repositories.Repairs;

public class AddRepairRepository : IAddRepairRepository
{
    private readonly DbContext _context;
    private readonly IAppLogger<AddRepairRepository> _logger;

    public AddRepairRepository(DbContext context, IAppLogger<AddRepairRepository> logger)
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
            string sql = $"SELECT \"id\", \"Інвентарний номер\", \"Бренд\", \"Модель\" FROM \"UserTables\".\"{equipmentTableName}\"; ";
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

    public async Task InsertUsedMaterialsAsync(ObservableCollection<RepairConsumableDto> repairConsumableDtos, int repairId, string consumablesTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            _logger.LogInformation("Inserting in database used materials for repair consumable");
            var sql = new StringBuilder($@"INSERT INTO ""UserTables"".""{consumablesTableName}"" (""Ремонт"", ""Категорія"", ""Назва"", ""Одиниця"", ""Витрачено"") VALUES ");
            var parameters = new List<NpgsqlParameter>();

            for (int i = 0; i < repairConsumableDtos.Count; i++)
            {
                if (i > 0) sql.Append(", ");
                sql.Append($"(@repairId, @category{i}, @name{i}, @unit{i}, @quantity{i})");
                parameters.Add(new NpgsqlParameter($"@category{i}", repairConsumableDtos[i].Category));
                parameters.Add(new NpgsqlParameter($"@name{i}", repairConsumableDtos[i].Name));
                parameters.Add(new NpgsqlParameter($"@unit{i}", repairConsumableDtos[i].Unit));
                parameters.Add(new NpgsqlParameter($"@quantity{i}", repairConsumableDtos[i].SpentMaterial));
            }
            parameters.Add(new NpgsqlParameter("@repairId", repairId));
            await using var cmd = new NpgsqlCommand(sql.ToString(), connection, transaction);
            cmd.Parameters.AddRange(parameters.ToArray());
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            _logger.LogInformation("Successfully inserting in database used materials for repair consumable");
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error inserting used materials");
            throw;
        }
    }
}