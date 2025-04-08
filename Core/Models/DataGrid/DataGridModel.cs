using System.Data;
using System.Dynamic;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using DbContext = Data.AppDbContext.DbContext;
using TableColumn = Data.Entities.TableColumn;

namespace Core.Models.DataGrid;

public class DataGridModel
{
    private readonly DbContext _context;

    private string _currentTableName;

    public DataGridModel(DbContext context)
    {
        Console.WriteLine("DataGridModel constructor");
        _context = context;
    }

    public void SetTableName(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name cannot be empty or null.", nameof(tableName));
        _currentTableName = tableName;
        Console.WriteLine($"Current table name: {_currentTableName}");
    }

    public async Task<List<TableColumn>> GetColumnNamesAsync()
    {
        if (string.IsNullOrEmpty(_currentTableName))
            throw new InvalidOperationException("Table name is not set.");

        var columns = new List<TableColumn>();

        try
        {
            var connection = _context.Database.GetDbConnection();
            Console.WriteLine("Before OpenAsync: " + connection.State);
            
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
                Console.WriteLine("After OpenAsync: " + connection.State);
            }

            var command = connection.CreateCommand();
            command.CommandText = $"SELECT * FROM \"UserTables\".\"{_currentTableName}\" LIMIT 0";

            var reader = await command.ExecuteReaderAsync();
            for (int i = 0; i < reader.FieldCount; i++)
                {
                    columns.Add(new TableColumn { ColumnName = reader.GetName(i) });
                }

            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting column names: {ex.Message}");
            throw;
        }

        return columns;
    }

    public async Task<List<dynamic>> GetTableDataAsync()
    {
        if (string.IsNullOrEmpty(_currentTableName))
            throw new InvalidOperationException("Table name is not set.");

        var result = new List<dynamic>();

        try
        {
            var connection = _context.Database.GetDbConnection() as NpgsqlConnection;
            if (connection.State != ConnectionState.Open)
            {
                await _context.Database.OpenConnectionAsync();
            }
            var command = new NpgsqlCommand($"SELECT * FROM \"UserTables\".\"{_currentTableName}\"", connection)
            {
                CommandTimeout = 30
            };

            var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                dynamic row = new ExpandoObject();
                var rowDict = (IDictionary<string, object>)row;
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    rowDict[reader.GetName(i)] = reader.GetValue(i) == DBNull.Value ? null : reader.GetValue(i);
                }
                result.Add(row);
            }
            await connection.CloseAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting table data: {ex.Message}");
            throw;
        }
        return result;
    }
}
