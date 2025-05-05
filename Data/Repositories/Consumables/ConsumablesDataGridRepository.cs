using System.Collections.ObjectModel;
using System.Dynamic;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Syncfusion.UI.Xaml.Grid;
using DataRow = System.Data.DataRow;
using DbContext = Data.AppDbContext.DbContext;

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
        
        public async Task<IAsyncEnumerable<ExpandoObject>> GetDataAsync(string tableName)
        {
           async IAsyncEnumerable<ExpandoObject> GetDataStream()
           {
               await using var connection = await OpenNewConnectionAsync();
               string query = $"SELECT * FROM \"ConsumablesSchema\".\"{tableName}\"";
               using var cmd = new NpgsqlCommand(query, connection);
        
               using var reader = await cmd.ExecuteReaderAsync();

               var columnNames = new List<string>();
               var schemaTable = reader.GetSchemaTable();
        
               if (schemaTable != null)
               {
                   foreach (DataRow row in schemaTable.Rows)
                   {
                       columnNames.Add(row["ColumnName"].ToString() ?? string.Empty);
                   }
               }

               while (await reader.ReadAsync())
               {
                   dynamic dataRow = new ExpandoObject();
                   var expandoDict = (IDictionary<string, object>)dataRow;

                   for (int i = 0; i < columnNames.Count; i++)
                   {
                       expandoDict[columnNames[i]] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                   }

                   yield return dataRow;
               }
           }
            return GetDataStream();
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
    }
}
