using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Models.RepairsDataGrid.ServicesDataGrid;
using Npgsql;
using DbContext = Data.AppDbContext.DbContext;

namespace Data.Repositories.Services;

public class ServicesDataGridRepository : IServicesDataGridReposotory
{
    private readonly AppDbContext.DbContext _context;
    private readonly IAppLogger<ServicesDataGridRepository> _logger;

    public ServicesDataGridRepository(DbContext context, IAppLogger<ServicesDataGridRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<List<ServiceDto>> GetDataAsync(string servicesTableName, string equipmentsTable)
    {
        var services = new List<ServiceDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        
        try
        {
            string sql =
                $@" SELECT r.""id"", r.""Об'єкт"", r.""Тип обслуговування"", r.""Опис обслуговування"", r.""Дата початку"", r.""Дата кінця"", r.""Витрачено часу"", r.""Працівник"", r.""Статус"", e.""Інвентарний номер"", e.""Бренд"", e.""Модель"" " +
                $@" FROM ""UserTables"".""{servicesTableName}"" r " +
                $@"JOIN ""UserTables"".""{equipmentsTable}"" e ON r.""Об'єкт"" = e.""id""; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    services.Add(new ServiceDto()
                    {
                        Id = reader.GetValueOrDefault<int>("id"),
                        EquipmentId = reader.GetValueOrDefault<int>("Об'єкт"),
                        Type = reader.GetValueOrDefault<string>("Тип обслуговування"),
                        ServiceDescription = reader.GetValueOrDefault<string>("Опис обслуговування"),
                        StartDate = reader.GetValueOrDefault<DateTime?>("Дата початку"),
                        EndDate = reader.GetValueOrDefault<DateTime?>("Дата кінця"),
                        Duration = reader.GetValueOrDefault<TimeSpan?>("Витрачено часу"),
                        Worker = reader.GetValueOrDefault<int>("Працівник"),
                        Status = reader.GetValueOrDefault<string>("Статус"),
                        EquipmentInventoryNumber = reader.GetValueOrDefault<string>("Інвентарний номер"),
                        EquipmentBrand = reader.GetValueOrDefault<string>("Бренд"),
                        EquipmentModel = reader.GetValueOrDefault<string>("Модель")
                    });
                }
            }
            return services;
        }
        catch (NpgsqlException e)
        {
            _logger.LogError(e,"Database error getting repairs");
            throw;
        }
    }
}