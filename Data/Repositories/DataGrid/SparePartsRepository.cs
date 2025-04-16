using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Common.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using DbContext = Data.AppDbContext.DbContext;

namespace Data.Repositories.DataGrid
{
    public class SparePartsRepository : ISparePartsRepository
    {
        private readonly DbContext _context;
        private readonly IAppLogger<SparePartsRepository> _logger;

        public SparePartsRepository(DbContext context, IAppLogger<SparePartsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName, object equipmentId)
        {
            var data = new ObservableCollection<ExpandoObject>();
            Console.WriteLine("GetDataAsync from repository, tdable name: " + tableName);
            
             try
            {
                _logger.LogInformation("Executing query to fetch data from table {TableName}", tableName);

                string query = $"SELECT * FROM \"UserTables\".\"{tableName}\" WHERE \"EquipmentId\" = @equipmentId";
                var connection = _context.Database.GetDbConnection() as NpgsqlConnection;
                
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                
                Console.WriteLine("SQL: "+query);
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.Add(new NpgsqlParameter("equipmentId", equipmentId));
                using var reader = await cmd.ExecuteReaderAsync();

                var columnNames = new List<string>();
                var schemaTable = reader.GetSchemaTable();
                if (schemaTable != null)
                {
                    foreach (DataRow row in schemaTable.Rows)
                    {
                        columnNames.Add(row["ColumnName"].ToString());
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

                    data.Add(dataRow);
                }

                _logger.LogInformation("Successfully fetched {Count} records from table {TableName}", data.Count,
                    tableName);
                return data;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error fetching data from table {TableName}", tableName);
                throw;
            }
        }
        
        public async Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName)
        {
            var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var connection = _context.Database.GetDbConnection() as NpgsqlConnection;

            try
            {
                _logger.LogInformation("Fetching column types for table {TableName}", tableName);
                if (connection.State != ConnectionState.Open)
                {
                    _logger.LogInformation("Database connection closed, opening");
                    await _context.Database.OpenConnectionAsync();
                }

                string sql = @"SELECT column_name, data_type 
                          FROM information_schema.columns 
                          WHERE table_schema = 'UserTables' AND table_name = @tableName";

                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.Add(new NpgsqlParameter("@tableName", tableName));
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    string columnName = reader["column_name"].ToString();
                    string dataType = reader["data_type"].ToString();
                    columnTypes[columnName] = dataType;
                }

                _logger.LogInformation("Retrieved {Count} column types for table {TableName}", columnTypes.Count, tableName);
                return columnTypes;
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error while fetching column types for table {TableName}. SqlState: {SqlState}", tableName, ex.SqlState);
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
