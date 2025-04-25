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
    public class DataGridRepository : IDataGridRepository
    {
        private readonly DbContext _context;
        private readonly IAppLogger<DataGridRepository> _logger;

        public DataGridRepository(DbContext context, IAppLogger<DataGridRepository> logger)
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
        
        #region GetDataAsync
        public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName)
        {
            var data = new ObservableCollection<ExpandoObject>();
            try
            {
                _logger.LogInformation("Executing query to fetch data from table {TableName}", tableName);

                await using var connection = await OpenNewConnectionAsync();
                
                string query = $"SELECT * FROM \"UserTables\".\"{tableName}\"";
                

                using var cmd = new NpgsqlCommand(query, connection);
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
        #endregion
        
        #region InsertRecordAsync
        public async Task<object> InsertRecordAsync(string tableName, Dictionary<string, object> values)
        {
            try
            {
                _logger.LogInformation("Inserting record into table {TableName}", tableName);
                
                await using var connection = await OpenNewConnectionAsync();

                if (values == null || values.Count == 0)
                {
                    _logger.LogWarning("No valid values provided for insert into table {TableName}", tableName);
                    return null;
                }

                values.Remove("id");
                if (values.Count == 0)
                {
                    _logger.LogWarning("No values to insert after removing ID for table {TableName}", tableName);
                    return null;
                }

                var columnTypes = await GetColumnTypesAsync(tableName);
                var columns = new List<string>();
                var paramPlaceholders = new List<string>();
                var parameters = new List<NpgsqlParameter>();

                int paramIndex = 0;
                foreach (var kvp in values)
                {
                    if (columnTypes.TryGetValue(kvp.Key, out string dataType))
                    {
                        columns.Add($"\"{kvp.Key}\"");
                        paramPlaceholders.Add($"@p{paramIndex}");
                        object convertedValue = ConvertValueToColumnType(kvp.Value, dataType);
                        parameters.Add(new NpgsqlParameter($"@p{paramIndex}", convertedValue ?? DBNull.Value)
                        {
                            NpgsqlDbType = GetNpgsqlDbType(dataType)
                        });
                        paramIndex++;
                    }
                    else
                    {
                        _logger.LogWarning("Column {ColumnName} not found in table {TableName}, skipping", kvp.Key,
                            tableName);
                    }
                }

                if (columns.Count == 0)
                {
                    _logger.LogWarning("No valid columns to insert for table {TableName}", tableName);
                    return null;
                }

                string sql = $"INSERT INTO \"UserTables\".\"{tableName}\" ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramPlaceholders)}) RETURNING \"id\"";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddRange(parameters.ToArray());

                var insertedId = await cmd.ExecuteScalarAsync();
                _logger.LogInformation("Inserted record with ID {Id} into table {TableName}", insertedId, tableName);
                return insertedId;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e,
                    "Database error while inserting record into table {TableName}. SqlState: {SqlState}", tableName,
                    e.SqlState);
                throw;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected error while inserting record into table {TableName}", tableName);
                throw;
            }
        }
        #endregion
        
        #region UpdateRecordAsync
        public async Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values)
        {
            try
            {
                _logger.LogInformation("Updating record with ID {Id} in table {TableName}", id, tableName);
                await using var connection = await OpenNewConnectionAsync();

                if (values == null || values.Count == 0)
                {
                    _logger.LogWarning("No values provided for update of record ID {Id} in table {TableName}", id,
                        tableName);
                    return;
                }

                var setClause = string.Join(", ", values.Keys.Select((k, i) => $"\"{k}\" = @p{i}"));
                var sql = $"UPDATE \"UserTables\".\"{tableName}\" SET {setClause} WHERE id = @id";

                using var cmd = new NpgsqlCommand(sql, connection);
                int paramIndex = 0;
                foreach (var kvp in values)
                {
                    object paramValue = kvp.Value ?? DBNull.Value;
                    NpgsqlDbType? paramType = null;

                    if (paramValue != DBNull.Value)
                    {
                        if (kvp.Value is string stringValue)
                        {
                            if (decimal.TryParse(stringValue, out var numericValue))
                            {
                                paramValue = numericValue;
                                paramType = NpgsqlDbType.Numeric;
                            }
                            else
                            {
                                paramType = NpgsqlDbType.Text;
                            }
                        }
                        else if (kvp.Value is decimal or double or float or int or long)
                        {
                            paramType = NpgsqlDbType.Numeric;
                        }
                        else
                        {
                            paramType = NpgsqlDbType.Unknown;
                        }
                    }

                    var param = new NpgsqlParameter($"@p{paramIndex}", paramType ?? NpgsqlDbType.Unknown)
                    {
                        Value = paramValue
                    };
                    cmd.Parameters.Add(param);
                    paramIndex++;
                }

                cmd.Parameters.AddWithValue("@id", id);
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Successfully updated record with ID {Id} in table {TableName}", id, tableName);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex,
                    "Database error while updating record ID {Id} in table {TableName}. SqlState: {SqlState}", id,
                    tableName, ex.SqlState);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while updating record ID {Id} in table {TableName}", id,
                    tableName);
                throw;
            }
        }
        #endregion
        
        #region DeleteRecordAsync
        public async Task DeleteRecordAsync(string tableName, object id)
        {
            try
            {
                _logger.LogInformation("Deleting record with ID {Id} from table {TableName}", id, tableName);
                
                await using var connection = await OpenNewConnectionAsync();

                var sql = $"DELETE FROM \"UserTables\".\"{tableName}\" WHERE \"id\" = @id";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.Add(new NpgsqlParameter("@id", id));
                await cmd.ExecuteNonQueryAsync();
                _logger.LogInformation("Successfully deleted record with ID {Id} from table {TableName}", id, tableName);
            }
            catch (NpgsqlException ex)
            {
                _logger.LogError(ex, "Database error while deleting record ID {Id} from table {TableName}. SqlState: {SqlState}", id, tableName, ex.SqlState);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while deleting record ID {Id} from table {TableName}", id, tableName);
                throw;
            }
        }
        #endregion
        
        #region GetColumnTypesAsync
        public async Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName)
        {
            var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            
            try
            {
                _logger.LogInformation("Fetching column types for table {TableName}", tableName);
                
                await using var connection = await OpenNewConnectionAsync();

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
        #endregion
        
        #region ConvertValueToColumnType
        private object ConvertValueToColumnType(object value, string dbType)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            if (value is string stringValue)
            {
                switch (dbType.ToLower())
                {
                    case "integer":
                    case "int":
                    case "int4":
                        return int.TryParse(stringValue, out int intValue) ? intValue : 0;

                    case "bigint":
                    case "int8":
                        return long.TryParse(stringValue, out long longValue) ? longValue : 0L;

                    case "numeric":
                    case "decimal":
                        stringValue = stringValue.Replace(',', '.');
                        return decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue) ? decimalValue : 0m;

                    case "real":
                    case "float4":
                        return float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue) ? floatValue : 0f;

                    case "double precision":
                    case "float8":
                        return double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue) ? doubleValue : 0d;

                    case "boolean":
                    case "bool":
                        return bool.TryParse(stringValue, out bool boolValue) ? boolValue : false;

                    case "date":
                    case "timestamp":
                    case "timestamptz":
                        return DateTime.TryParse(stringValue, out DateTime dateValue) ? dateValue : DateTime.Now;

                    default:
                        return stringValue;
                }
            }

            return value;
        }
        #endregion
        
        #region GetNpgsqlDbType
        private NpgsqlDbType GetNpgsqlDbType(string dataType)
        {
            switch (dataType.ToLower())
            {
                case "integer":
                case "int":
                case "int4":
                    return NpgsqlDbType.Integer;

                case "bigint":
                case "int8":
                    return NpgsqlDbType.Bigint;

                case "numeric":
                case "decimal":
                    return NpgsqlDbType.Numeric;

                case "real":
                case "float4":
                    return NpgsqlDbType.Real;

                case "double precision":
                case "float8":
                    return NpgsqlDbType.Double;

                case "boolean":
                case "bool":
                    return NpgsqlDbType.Boolean;

                case "date":
                    return NpgsqlDbType.Date;

                case "timestamp":
                    return NpgsqlDbType.Timestamp;

                case "timestamptz":
                    return NpgsqlDbType.TimestampTz;

                case "varchar":
                case "character varying":
                    return NpgsqlDbType.Varchar;

                case "text":
                    return NpgsqlDbType.Text;

                default:
                    return NpgsqlDbType.Text;
            }
        }
        #endregion
    }
}
