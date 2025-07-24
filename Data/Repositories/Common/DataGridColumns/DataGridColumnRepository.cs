using Common.Logging;
using Data.ApplicationDbContext;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Data.Repositories.Common.DataGridColumns
{
    public class DataGridColumnRepository : IDataGridColumnRepository
    {
        private IAppLogger<DataGridColumnRepository> _logger;
        private AppDbContext _context;

        public DataGridColumnRepository(IAppLogger<DataGridColumnRepository> logger, AppDbContext context)
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
        
        public async Task<Dictionary<string, string>> GetColumnTypesAsync(string schema, string tableName)
        {
            var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                _logger.LogInformation("Fetching column types for table {TableName}", tableName);
                
                await using var connection = await OpenNewConnectionAsync();
                _logger.LogInformation("Opened connection to database.");

                string sql = @"SELECT column_name, data_type 
                          FROM information_schema.columns 
                          WHERE table_schema = @schema AND table_name = @tableName";      
                
                _logger.LogInformation("SQL : {sql}", sql);

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.Add(new NpgsqlParameter("@schema", schema));
                cmd.Parameters.Add(new NpgsqlParameter("@tableName", tableName));

                var reader = await cmd.ExecuteReaderAsync();
                

                while (await reader.ReadAsync())
                {
                    string columnName = reader["column_name"].ToString();
                    string dataType = reader["data_type"].ToString();
                    columnTypes[columnName] = dataType;
                }

                _logger.LogInformation("Retrieved {Count} column types for table {TableName}", columnTypes.Count,
                    tableName);
                return columnTypes;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Database error while fetching column types for table {TableName}. SqlState: {SqlState}", tableName,
                    ex.SqlState);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while fetching column types for table {TableName}", tableName);
                throw;
            }
        }
    }
}

