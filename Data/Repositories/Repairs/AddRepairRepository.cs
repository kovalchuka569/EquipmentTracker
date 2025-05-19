using Common.Logging;
using Data.AppDbContext;
using Models.ConsumablesDataGrid;
using Models.RepairsDataGrid.AddRepair;
using Npgsql;

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

    public Task SaveRepairAsync(EquipmentDto equipment, ConsumableDto consumable)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (NpgsqlException e)
        {
            _logger.LogError(e, "Database error saving repair");
            throw;
        }
    }
}