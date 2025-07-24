using System.Collections.ObjectModel;
using Common.Logging;
using Data.ApplicationDbContext;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Models.RepairsDataGrid.AddService;
using Npgsql;

namespace Data.Repositories.Repairs;
 
public class RepairsRepository : IRepairsRepository
{
    private readonly AppDbContext _context;
    private readonly IAppLogger<RepairsRepository> _logger;

    public RepairsRepository(AppDbContext context, IAppLogger<RepairsRepository> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<List<RepairDto>> GetDataAsync(string repairsTable, string equipmentTable)
    {
        var repairs = new List<RepairDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        try
        {
            string sql =
                $"SELECT r.\"id\", r.\"Об'єкт\", r.\"Опис поломки\", r.\"Дата початку\", r.\"Дата кінця\", r.\"Витрачено часу\", r.\"Працівник\", r.\"Статус\", e.\"Інвентарний номер\", e.\"Бренд\", e.\"Модель\" " +
                $"FROM \"UserTables\".\"{repairsTable}\" r " +
                $"JOIN \"UserTables\".\"{equipmentTable}\" e ON r.\"Об'єкт\" = e.\"id\"; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    repairs.Add(new RepairDto
                    {
                        Id = reader.GetValueOrDefault<int>("id"),
                        EquipmentId = reader.GetValueOrDefault<int>("Об'єкт"),
                        BreakDescription = reader.GetValueOrDefault<string>("Опис поломки"),
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
            return repairs;
        }
        catch (NpgsqlException e)
        {
            _logger.LogError(e,"Database error getting repairs");
            throw;
        }
    }
}