using Common.Logging;
using Data.AppDbContext;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;
using Models.RepairsDataGrid.ServicesDataGrid;
using Npgsql;
using NpgsqlTypes;

namespace Data.Repositories.Services;

public class AddServiceRepository : IAddServiceRepository
{
    private readonly DbContext _context;
    private readonly IAppLogger<AddServiceRepository> _logger;

    public AddServiceRepository(DbContext context, IAppLogger<AddServiceRepository> logger)
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
                        EquipmentId = reader.GetValueOrDefault<int>("id"),
                        EquipmentInventoryNumber = reader.GetValueOrDefault<string>("Інвентарний номер"),
                        EquipmentBrand = reader.GetValueOrDefault<string>("Бренд"),
                        EquipmentModel = reader.GetValueOrDefault<string>("Модель"),
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

    public async Task<int> SaveServiceAsync(ServiceData serviceData, string servicesTableName)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql =
                $@"INSERT INTO ""UserTables"".""{servicesTableName}"" (""Об'єкт"", ""Тип обслуговування"", ""Опис обслуговування"", ""Дата початку"", ""Дата кінця"", ""Витрачено часу"", ""Працівник"", ""Статус"") " +
                "VALUES (@object, @serviceType, @serviceDescription, @dateStart, @dateEnd, @spentTime, @worker, @status) RETURNING id; ";
            using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@object", serviceData.EquipmentId);
            cmd.Parameters.AddWithValue("@serviceType", serviceData.ServiceType);
            cmd.Parameters.AddWithValue("@serviceDescription", NpgsqlDbType.Text).Value = serviceData.ServiceDescription ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@dateStart", NpgsqlDbType.Date).Value = serviceData.StartService ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@dateEnd", NpgsqlDbType.Date).Value = serviceData.EndService ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@spentTime", NpgsqlDbType.Timestamp).Value = serviceData.TimeSpentOnService ?? (object)DBNull.Value;
            cmd.Parameters.AddWithValue("@worker", serviceData.Worker);
            cmd.Parameters.AddWithValue("@status", serviceData.ServiceStatus);
            var result = await cmd.ExecuteScalarAsync();
            await transaction.CommitAsync();
            int id = Convert.ToInt32(result);
            return id;
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database error save service");
            throw;
        }
    }

    public async Task UpdateServiceAsync(ServiceData serviceData, string servicesTableName, int serviceId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = connection.BeginTransaction();
        try
        {
            string sql = $@"UPDATE ""UserTables"".""{servicesTableName}"" 
                                                            SET 
                                                            ""Опис обслуговування"" = @description, 
                                                            ""Дата початку"" = @dateTimeStart,
                                                            ""Дата кінця"" = @dateTimeEnd,
                                                            ""Витрачено часу"" = @spentTime,
                                                            ""Тип обслуговування"" = @serviceType,
                                                            ""Працівник"" = @worker,
                                                            ""Статус"" = @serviceStatus 
                                                            WHERE ""id"" = @serviceId; ";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@serviceId", serviceId);
            cmd.Parameters.AddWithNullableValue("@dateTimeStart", serviceData.StartService, NpgsqlDbType.Timestamp);
            cmd.Parameters.AddWithNullableValue("@dateTimeEnd", serviceData.EndService, NpgsqlDbType.Timestamp);
            cmd.Parameters.AddWithNullableValue("@spentTime", serviceData.TimeSpentOnService, NpgsqlDbType.Interval);
            cmd.Parameters.AddWithNullableValue("@description", serviceData.ServiceDescription, NpgsqlDbType.Text);
            cmd.Parameters.AddWithValue("@serviceType", serviceData.ServiceType);
            cmd.Parameters.AddWithValue("@worker", serviceData.Worker);
            cmd.Parameters.AddWithValue("@serviceStatus", serviceData.ServiceStatus);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            _logger.LogError(e, "Database failed to update service");
            throw;
        }
    }
}