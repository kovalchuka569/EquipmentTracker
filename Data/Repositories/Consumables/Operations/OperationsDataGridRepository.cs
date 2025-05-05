using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using DbContext = Data.AppDbContext.DbContext;

namespace Data.Repositories.Consumables.Operations
{
    public class OperationsDataGridRepository : IOperationsDataGridRepository
    {
        private IAppLogger<OperationsDataGridRepository> _logger;
        private DbContext _context;
        
        public OperationsDataGridRepository(IAppLogger<OperationsDataGridRepository> logger, DbContext context)
        {
            _logger = logger;
            _context = context;
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
        
        public async Task<IAsyncEnumerable<ExpandoObject>> LoadDataAsync(string tableName, int materialId)
        {
            async IAsyncEnumerable<ExpandoObject> GetDataStream()
            {
                await using var connection = await OpenNewConnectionAsync();
                string query = $"SELECT * FROM \"ConsumablesSchema\".\"{tableName}\" WHERE \"Матеріал\" = @materialId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@materialId", materialId);
                
                Console.WriteLine(query);
                
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

        public async Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user)
        
        {
            var parsedQuantity = decimal.Parse(quantity);
            var parsedDateTime = DateTime.Parse(dateTime);
            
            await using var connection = await OpenNewConnectionAsync();
            string query = $"INSERT INTO \"ConsumablesSchema\".\"{tableName}\" (\"Матеріал\", \"Кількість\", \"Тип операції\", \"Дата, час\", \"Опис\", \"Користувач\") VALUES (@materialId, @quantity, @operationType, @datetime, @description, @user) RETURNING \"id\"";
            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@materialId", materialId);
            cmd.Parameters.AddWithValue("@quantity", parsedQuantity);
            cmd.Parameters.AddWithValue("@operationType", operationType);
            cmd.Parameters.AddWithValue("@datetime", parsedDateTime);
            cmd.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? DBNull.Value : description);
            cmd.Parameters.AddWithValue("@user", user);
            var newId = (int)await cmd.ExecuteScalarAsync();
            return newId;
        }
    }
}
