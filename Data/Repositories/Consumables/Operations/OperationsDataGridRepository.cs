using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
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
        
        public async Task<List<OperationDto>> LoadDataAsync(string tableName, int materialId)
        {
            var result = new List<OperationDto>();
            {
                await using var connection = await OpenNewConnectionAsync();
                string query = $"SELECT * FROM \"ConsumablesSchema\".\"{tableName}\" WHERE \"Матеріал\" = @materialId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@materialId", materialId);
                
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var operation = new OperationDto
                    {
                        Id = reader.GetInt32(reader.GetOrdinal("id")),
                        OperationType = reader["Тип операції"]?.ToString(),
                        Quantity = reader.GetDouble(reader.GetOrdinal("Кількість")),
                        Description = reader["Опис"]?.ToString(),
                        Worker = reader["Користувач"]?.ToString(),
                        DateTime = reader.GetDateTime(reader.GetOrdinal("Дата, час")),
                        Receipt = reader["Квитанція"] as byte[]
                    };
                    result.Add(operation);
                }
                return result;
            }
        }

        public async Task<int> InsertRecordAsync(string tableName, int materialId, string operationType,
            string dateTime, string quantity, string description, int user, byte[] receiptImageBytes)
        
        {
            var parsedQuantity = decimal.Parse(quantity);
            var parsedDateTime = DateTime.Parse(dateTime);
            
            await using var connection = await OpenNewConnectionAsync();
            string query = $"INSERT INTO \"ConsumablesSchema\".\"{tableName}\" (\"Матеріал\", \"Кількість\", \"Тип операції\", \"Дата, час\", \"Квитанція\", \"Опис\", \"Користувач\") VALUES (@materialId, @quantity, @operationType, @datetime, @receipt, @description, @user) RETURNING \"id\"";
            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@materialId", materialId);
            cmd.Parameters.AddWithValue("@quantity", parsedQuantity);
            cmd.Parameters.AddWithValue("@operationType", operationType);
            cmd.Parameters.AddWithValue("@datetime", parsedDateTime);
            AddNullableParameter(cmd, "@receipt", receiptImageBytes, NpgsqlDbType.Bytea);
            AddNullableParameter(cmd, "@description", description, NpgsqlDbType.Text);
            cmd.Parameters.AddWithValue("@user", user);
            var newId = (int)await cmd.ExecuteScalarAsync();
            return newId;
        }
        
        private void AddNullableParameter(NpgsqlCommand cmd, string name, object value, NpgsqlDbType type)
        {
            if (value == null)
                cmd.Parameters.Add(name, type).Value = DBNull.Value;
            else
                cmd.Parameters.AddWithValue(name, value);
        }
    }
}
