using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using DbContext = Data.AppDbContext.DbContext;

namespace Core.Models.DataGrid;

public class DataGridModel
{
    private readonly DbContext _context;
    
    /// <summary>
    /// Initializes a new instance of the DataGridModel class
    /// </summary>
    /// <param name="context">Database context</param>
    public DataGridModel(DbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Gets data rows from the specified table in the UserTables schema
    /// </summary>
    /// <param name="tableName">Name of the table in UserTables schema</param>
    /// <returns>Collection of dynamic objects representing table data</returns>
    public async Task<ObservableCollection<ExpandoObject>> GetDataAsync(string tableName)
    {
        var data = new ObservableCollection<ExpandoObject>();
        
        var connection = _context.Database.GetDbConnection() as NpgsqlConnection;
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            string query = $"SELECT * FROM \"UserTables\".\"{tableName}\"";
                
            using (var cmd = new NpgsqlCommand(query, connection))
            {
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    DataTable schemaTable = reader.GetSchemaTable();
                    var columnNames = new List<string>();
                    var columnTypes = new List<Type>();
                    
                    if (schemaTable != null)
                    {
                        foreach (System.Data.DataRow row in schemaTable.Rows)
                        {
                            columnNames.Add(row["ColumnName"].ToString());
                            columnTypes.Add((Type)row["DataType"]);
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
                }
            }
        }
        catch (Exception ex)
        {
            // Log exception
            throw;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
        
        return data;
    }

    public async Task<object> InsertRecordAsync(string tableName, Dictionary<string, object> values)
    {
        if (values == null || values.Count == 0)
        {
            return null;
        }
        using var connection = new NpgsqlConnection(_context.Database.GetConnectionString());       
        try
        {
            await connection.OpenAsync();

            values.Remove("id");
            if (values.Count == 0)
            {
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

                    // Convert value to the appropriate type based on the column data type
                    object convertedValue = ConvertValueToColumnType(kvp.Value, dataType);
                    parameters.Add(new NpgsqlParameter($"@p{paramIndex}", convertedValue ?? DBNull.Value)
                    {
                        // Explicitly set NpgsqlDbType if necessary
                        NpgsqlDbType = GetNpgsqlDbType(dataType)
                    });

                    paramIndex++;
                }
            }

            string sql = $"INSERT INTO \"UserTables\".\"{tableName}\" ({string.Join(", ", columns)}) VALUES ({string.Join(", ", paramPlaceholders)}) RETURNING \"id\"";

            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                foreach (var param in parameters)
                {
                    cmd.Parameters.Add(param);
                }

                return await cmd.ExecuteScalarAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
    }

    public async Task DeleteRecordAsync(string tableName, object id)
    {
        var connection = _context.Database.GetDbConnection() as NpgsqlConnection;

        try
        { 
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
        
            var sql = $"DELETE FROM \"UserTables\".\"{tableName}\" WHERE \"id\" = @id";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.Add(new NpgsqlParameter("id", id));
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
                await connection.CloseAsync();
        }
    }

   public async Task UpdateRecordAsync(string tableName, object id, Dictionary<string, object> values)
    {
        var connection = _context.Database.GetDbConnection() as NpgsqlConnection;
        try
        { 
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            
            var setClause = string.Join(", ", values.Keys.Select((k, i) => $"\"{k}\" = @p{i}"));
            var sql = $"UPDATE \"UserTables\".\"{tableName}\" SET {setClause} WHERE id = @id";
            
            Console.WriteLine($"SQL Query: {sql}");
            Console.WriteLine($"Parameters: id = {id}, values = {string.Join(", ", values.Select(kv => $"{kv.Key}: {kv.Value ?? "null"} (Type: {kv.Value?.GetType()?.Name ?? "null"})"))}");

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
                var param = new NpgsqlParameter($"p{paramIndex}", paramType ?? NpgsqlDbType.Unknown)
                {
                    Value = paramValue
                };
                cmd.Parameters.Add(param);
                paramIndex++;
            }
            
            cmd.Parameters.AddWithValue("id", id);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (NpgsqlException ex)
        {
            Console.WriteLine($"Database error: {ex.Message}, SqlState: {ex.SqlState}");
            throw;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
}

    public async Task<Dictionary<string, string>> GetColumnTypesAsync(string tableName)
    {
        var columnTypes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var connection = _context.Database.GetDbConnection() as NpgsqlConnection;
        try
        {
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }

            string sql =
                @"SELECT column_name, data_type FROM information_schema.columns WHERE table_schema = 'UserTables' AND table_name = @tableName";
            using (var cmd = new NpgsqlCommand(sql, connection))
            {
                cmd.Parameters.Add(new NpgsqlParameter("tableName", tableName));
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string columnName = reader["column_name"].ToString();
                        string dataType = reader["data_type"].ToString();
                        columnTypes[columnName] = dataType;
                    }
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            if (connection.State == ConnectionState.Open)
                await connection.CloseAsync();
        }
        return columnTypes;
    }
    /// <summary>
/// Converts a value to the appropriate type based on the database column type
/// </summary>
/// <param name="value">Original value</param>
/// <param name="dbType">Database column type</param>
/// <returns>Converted value</returns>
private object ConvertValueToColumnType(object value, string dbType)
{
    if (value == null)
        return DBNull.Value;
        
    // Convert string value to appropriate type
    if (value is string stringValue)
    {
        switch (dbType.ToLower())
        {
            case "integer":
            case "int":
            case "int4":
                if (int.TryParse(stringValue, out int intValue))
                    return intValue;
                return 0;
                
            case "bigint":
            case "int8":
                if (long.TryParse(stringValue, out long longValue))
                    return longValue;
                return 0L;
                
            case "numeric":
            case "decimal":
                // Handle different culture formats (comma vs dot)
                stringValue = stringValue.Replace(',', '.');
                if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decimalValue))
                    return decimalValue;
                return 0m;
                
            case "real":
            case "float4":
                if (float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out float floatValue))
                    return floatValue;
                return 0f;
                
            case "double precision":
            case "float8":
                if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double doubleValue))
                    return doubleValue;
                return 0d;
                
            case "boolean":
            case "bool":
                if (bool.TryParse(stringValue, out bool boolValue))
                    return boolValue;
                return false;
                
            case "date":
            case "timestamp":
            case "timestamptz":
                if (DateTime.TryParse(stringValue, out DateTime dateValue))
                    return dateValue;
                return DateTime.Now;
                
            default:
                return stringValue;
        }
    }
    
    return value;
}

/// <summary>
/// Gets the NpgsqlDbType from PostgreSQL data type string
/// </summary>
/// <param name="dataType">PostgreSQL data type</param>
/// <returns>NpgsqlDbType value</returns>
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
}

