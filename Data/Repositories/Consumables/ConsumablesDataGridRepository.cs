using System.Collections.ObjectModel;
using System.Dynamic;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Models.ConsumablesDataGrid;
using Npgsql;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Windows.Controls;
using DataRow = System.Data.DataRow;

namespace Data.Repositories.Consumables
{
    public class ConsumablesDataGridRepository : IConsumablesDataGridRepository
    {
        private readonly DbContext _context;
        private readonly IAppLogger<ConsumablesDataGridRepository> _logger;

        public ConsumablesDataGridRepository(DbContext context, IAppLogger<ConsumablesDataGridRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        private async Task<NpgsqlConnection> OpenNewConnectionAsync()
        {
            try
            {
                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                
                var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
        
                return connection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening database connection.");
                throw;
            }
        }
        
        public async Task<List<ConsumableDto>> GetDataAsync(string tableName)
        {
            var result = new List<ConsumableDto>();
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = $"SELECT * FROM  \"ConsumablesSchema\".\"{tableName}\"";
                using var cmd = new NpgsqlCommand(sql, connection);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var consumable = new ConsumableDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        Name = reader["Назва"]?.ToString(),
                        Category = reader["Категорія"]?.ToString(),
                        Balance = reader.GetDecimal(reader.GetOrdinal("Залишок")),
                        Unit = reader["Одиниця"]?.ToString(),
                        MinBalance = reader.GetDecimal(reader.GetOrdinal("Мінімальний залишок")),
                        MaxBalance = reader.GetDecimal(reader.GetOrdinal("Максимальний залишок")),
                        LastModifiedDate = reader.IsDBNull(reader.GetOrdinal("Дата, час останньої зміни")) ? null : reader.GetDateTime(reader.GetOrdinal("Дата, час останньої зміни")),
                        Notes = reader["Примітки"]?.ToString(),
                    };
                    result.Add(consumable);
                }
                return result;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e, "Database error getting data from {TableName}", tableName);
                throw;
            }
        }

        public async IAsyncEnumerable<string> StartListeningForChangesAsync(CancellationToken cancellationToken)
        {
            await using var connection = await OpenNewConnectionAsync();
            using var cmd = new NpgsqlCommand("LISTEN data_changed", connection);
            await cmd.ExecuteNonQueryAsync();
            _logger.LogInformation("Listening for data changes");

            var tcs = new TaskCompletionSource<string>();
            NotificationEventHandler handler = (o, e) =>
            {
                _logger.LogInformation("Notification received: {Payload}", e.Payload);
                tcs.TrySetResult(e.Payload);
            };
    
            connection.Notification += handler;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    tcs = new TaskCompletionSource<string>();
                    await connection.WaitAsync(cancellationToken);
                    var payload = await tcs.Task;
                    yield return payload;
                }
            }
            finally
            {
                connection.Notification -= handler;
            }
        }

        public async Task<(int? min, int? max)> GetDataMinMaxAsync(string tableName, int recordId)
        {
            await using var connection = await OpenNewConnectionAsync();
            string sql =
                $"SELECT \"Мінімальний залишок\", \"Максимальний залишок\" FROM \"ConsumablesSchema\".\"{tableName}\" WHERE \"id\" = {recordId}";
            
            await using var cmd = new NpgsqlCommand(sql, connection);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                int? min = reader.IsDBNull(0) ? null : reader.GetInt32(0);
                int? max = reader.IsDBNull(1) ? null : reader.GetInt32(1);
                return (min, max);
            }
            return (0, 0);
        }

        public async Task UpdateMinLevelAsync(string tableName, int recordId, int? min)
        {
            await using var connection = await OpenNewConnectionAsync();
            string sql = $"UPDATE \"ConsumablesSchema\".\"{tableName}\" SET \"Мінімальний залишок\" = {min} WHERE \"id\" = {recordId}";
            await using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }
        
        public async Task UpdateMaxLevelAsync(string tableName, int recordId, int? max)
        {
            await using var connection = await OpenNewConnectionAsync();
            string sql = $"UPDATE \"ConsumablesSchema\".\"{tableName}\" SET \"Максимальний залишок\" = {max} WHERE \"id\" = {recordId}";
            await using var cmd = new NpgsqlCommand(sql, connection);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DecreaseQuantityAsync(string tableName, int materialId, string quantity)
        {
            var parsedQuantity = decimal.Parse(quantity);
            
            await using var connection = await OpenNewConnectionAsync();
            string sql = $"UPDATE \"ConsumablesSchema\".\"{tableName}\" SET \"Залишок\" = \"Залишок\" - @quantity WHERE \"id\" = @materialId";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@materialId", materialId);
            cmd.Parameters.AddWithValue("@quantity", parsedQuantity);
            int newQuantity = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine(newQuantity);
            return newQuantity;
        }

        public async Task<int> IncreaseQuantityAsync(string tableName, int materialId, string quantity)
        {
            var parsedQuantity = decimal.Parse(quantity);
            
            await using var connection = await OpenNewConnectionAsync();
            string sql = $"UPDATE \"ConsumablesSchema\".\"{tableName}\" SET \"Залишок\" = \"Залишок\" + @quantity WHERE \"id\" = @materialId";
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@materialId", materialId);
            cmd.Parameters.AddWithValue("@quantity", parsedQuantity);
            int newQuantity = await cmd.ExecuteNonQueryAsync();
            Console.WriteLine(newQuantity);
            return newQuantity;
        }

        public async Task InsertConsumableAsync(ConsumableDto consumable, string tableName)
        {
            await using var connection = await OpenNewConnectionAsync();
            await using var transaction = connection.BeginTransaction();
            try
            {
                string sql =
                    $@"INSERT INTO ""ConsumablesSchema"".""{tableName}"" (""Назва"", ""Категорія"", ""Одиниця"", ""Примітки"") VALUES (@name, @category, @unit, @notes); ";
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", consumable.Name);
                cmd.Parameters.AddWithValue("@category", consumable.Category);
                cmd.Parameters.AddWithValue("@unit", consumable.Unit);
                cmd.Parameters.AddWithValue("@notes", consumable.Notes ?? (object)DBNull.Value);
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (NpgsqlException e)
            {
                transaction.RollbackAsync();
                _logger.LogError(e, "Database error inserting consumable");
                throw;
            }
        }

        public async Task UpdateConsumableAsync(ConsumableDto consumable, string tableName)
        {
            await using var connection = await OpenNewConnectionAsync();
            await using var transaction = connection.BeginTransaction();
            try
            {
                string sql = $@"UPDATE ""ConsumablesSchema"".""{tableName}"" SET 
                                                                            ""Назва"" = @name,
                                                                            ""Категорія"" = @category,
                                                                            ""Одиниця"" = @unit,
                                                                            ""Примітки"" = @notes 
                                                                            WHERE ""id"" = @id; ";
                
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", consumable.Name);
                cmd.Parameters.AddWithValue("@category", consumable.Category);
                cmd.Parameters.AddWithValue("@unit", consumable.Unit);
                cmd.Parameters.AddWithValue("@notes", consumable.Notes ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@id", consumable.Id);
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (NpgsqlException e)
            {
                transaction.RollbackAsync();
                _logger.LogError(e, "Database error updating consumable");
                throw;
            }
        }
    }
}
