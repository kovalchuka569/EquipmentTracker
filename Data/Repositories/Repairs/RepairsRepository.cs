using System.Collections.ObjectModel;
using Common.Logging;
using Data.AppDbContext;
using Models.RepairsDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Npgsql;

namespace Data.Repositories.Repairs;
 
public class RepairsRepository : IRepairsRepository
{
    private readonly DbContext _context;
    private readonly IAppLogger<RepairsRepository> _logger;

    public RepairsRepository(DbContext context, IAppLogger<RepairsRepository> logger)
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
                $"SELECT r.\"id\", r.\"Об'єкт\", r.\"Опис поломки\", r.\"Дата початку\", r.\"Дата кінця\", r.\"Працівник\", r.\"Статус\", e.\"Інвентарний номер\", e.\"Бренд\", e.\"Модель\" " +
                $"FROM \"UserTables\".\"{repairsTable}\" r " +
                $"JOIN \"UserTables\".\"{equipmentTable}\" e ON r.\"Об'єкт\" = e.\"id\"; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    repairs.Add(new RepairDto
                    {
                        Id = reader.GetInt32(0),
                        EquipmentId = reader.GetInt32(1),
                        RepairDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                        StartDate = reader.GetDateTime(3),
                        Worker = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        Status = reader.GetString(5),
                        EquipmentInventoryNumber = reader.GetString(6),
                        EquipmentBrand = reader.IsDBNull(7) ? null : reader.GetString(7),
                        EquipmentModel = reader.IsDBNull(8) ? null : reader.GetString(8),
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