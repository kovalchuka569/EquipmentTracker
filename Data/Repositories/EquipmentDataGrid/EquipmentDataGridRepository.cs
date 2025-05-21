using System.Collections.ObjectModel;
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
    
    public async Task<ObservableCollection<EquipmentDto>> GetDataAsync(string equipmentTableName)
    {
        var equipmentDto = new ObservableCollection<EquipmentDto>();
        var map = new Dictionary<string, string>
        {
            ["Інвентарний номер"] = "InventoryNumber",
            ["Бренд"] = "Brand",
            ["Модель"] = "Model",
            ["Серійнин номер"] = "SerialNumber",
            ["Клас"] = "Class",
            ["Рік"] = "Year",
            ["Висота (см)"] = "Height",
            ["Ширина (см)"] = "Width",
            ["Довжина (см)"] = "Length",
            ["Вага (кг)"] = "Weight",
            ["Поверх"] = "Floor",
            ["Відділ"] = "Department",
            ["Кімната"] = "Room",
            ["Споживання (кв/год)"] = "Consumption",
            ["Напруга (В)"] = "Room",
            ["Вода (л/год)"] = "Room",
            ["Повітря (л/год)"] = "Room",
            ["Балансова вартість (грн)"] = "Room",
            ["Кімната"] = "Room",
            ["Кімната"] = "Room",
            ["Кімната"] = "Room",
        };
        await using var connection = await _context.OpenNewConnectionAsync();
        try
        {
            string sql = $"SELECT * FROM \"UserTables\".\"{equipmentTableName}\"; ";
            using var cmd = new NpgsqlCommand(sql, connection);
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                var columnNames = Enumerable.Range(0, reader.FieldCount)
                    .Select(reader.GetName)
                    .ToHashSet();
                while (await reader.ReadAsync())
                {
                    
                }
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}