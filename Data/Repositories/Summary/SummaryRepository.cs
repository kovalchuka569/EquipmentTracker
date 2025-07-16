using Common.Logging;
using Data.AppDbContext;
using Models.Equipment;
using Models.Equipment.ColumnSpecificSettings;
using Models.EquipmentTree;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using ColumnDto = Models.Summary.ColumnTree.ColumnDto;
using ColumnItem = Models.Summary.DataGrid.ColumnItem;
using FileDto = Models.Summary.ColumnTree.FileDto;
using FolderDto = Models.Summary.ColumnTree.FolderDto;
using FolderItem = Models.Summary.ColumnTree.FolderItem;

namespace Data.Repositories.Summary;

public class SummaryRepository : ISummaryRepository
{
    private readonly IAppLogger<SummaryRepository> _logger;
    private readonly DbContext _context;

    public SummaryRepository(IAppLogger<SummaryRepository> logger, DbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    public async Task<SummaryFormat> GetSummaryFormat(int summaryId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        string sql = @"SELECT ""summary_format"" FROM ""public"".""summaries"" WHERE ""id"" = @summaryId;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@summaryId", summaryId);
        var summaryFormat = (SummaryFormat)await cmd.ExecuteScalarAsync();
        return summaryFormat;
    }

    public async Task<List<FolderDto>> GetFoldersAsync()
    {
        var folders = new List<FolderDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        string sql = @"SELECT * FROM ""public"".""folders""";
        await using var cmd = new NpgsqlCommand(sql, connection);
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                folders.Add(new FolderDto
                {
                    Id = reader.GetValueOrDefault<int>("id"),
                    Name = reader.GetValueOrDefault<string>("name"),
                    ParentId = reader.GetValueOrDefault<int>("parent_id"),
                });
            }
            return folders;
        }
    }

    public async Task<List<FileDto>> GetEquipmentFilesAsync()
    {
        var files = new List<FileDto>();
        await using var connection = await _context.OpenNewConnectionAsync();
        string sql = @"SELECT * FROM ""public"".""files"" WHERE ""file_format"" = 0";
        await using var cmd = new NpgsqlCommand(sql, connection);
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                files.Add(new FileDto
                {
                    Id = reader.GetValueOrDefault<int>("id"),
                    Name = reader.GetValueOrDefault<string>("name"),
                    FolderId = reader.GetValueOrDefault<int>("folder_id"),
                    TableId = reader.GetValueOrDefault<int>("table_id"),
                });
            }
            return files;
        }
    }

    public async Task<List<ColumnDto>> GetColumnsForTreeAsync(int tableId)
    {
        var columns = new List<ColumnDto>();
        using var connection = await _context.OpenNewConnectionAsync();
        string sql = @"SELECT settings->>'HeaderText' AS ""header_text"", ""id"" FROM ""public"".""custom_columns"" WHERE ""table_id"" = @tableId;";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@tableId", tableId);
        await using (var reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                columns.Add(new ColumnDto
                {
                    Id = reader.GetValueOrDefault<int>("id"),
                    HeaderText = reader.GetValueOrDefault<string>("header_text"),
                });
            }
            return columns;
        }
    }

    public async Task<List<ColumnItem>> GetColumnItemsForEquipmentsAsync(List<int> columnIds)
    {
        var columns = new List<(int Id, int TableId, string Settings)>();
        await using var connection = await _context.OpenNewConnectionAsync();
        const string sql = @"SELECT ""settings"", ""table_id"", ""id"" FROM ""public"".""custom_columns"" WHERE ""id"" = @columnId;";
        foreach (var id in columnIds)
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@columnId", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                columns.Add((
                    reader.GetValueOrDefault<int>("id"),
                    reader.GetValueOrDefault<int>("table_id"),
                    reader.GetValueOrDefault<string>("settings")
                ));
            }
        }
        return await Task.Run(() =>
        {
            var result = new List<ColumnItem>(columns.Count);
            foreach (var (id, tableId, settingsJson) in columns)
            {
                ColumnSettings settings = null;
                if (!string.IsNullOrWhiteSpace(settingsJson))
                {
                    settings = JsonConvert.DeserializeObject<ColumnSettings>(settingsJson);

                    if (settings?.SpecificSettings is JObject jObj)
                    {
                        settings.SpecificSettings = DeserializeSpecificSettings(settings.DataType, jObj);
                    }
                }

                result.Add(new ColumnItem()
                {
                    Id = id,
                    TableId = tableId,
                    ColumnSettings = settings
                });
            }

            return result;
        });
    }

    public async Task<List<(string, string)>> GetTableNamesForEquipmentSummaryAsync(List<int> columnIds)
    {
        var list = new List<(string, string)>();
        await using var connection = await _context.OpenNewConnectionAsync();
        const string sql = @"
                            SELECT 
                                f.""name"" AS table_name,
                                cc.""settings""->>'MappingName' AS mapping_name
                            FROM 
                                ""public"".""custom_columns"" cc
                            JOIN 
                                ""public"".""files"" f ON f.""table_id"" = cc.""table_id""
                            WHERE 
                                cc.""id"" = ANY(@ids);
                        ";
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@ids", columnIds);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var tableName = reader.GetValueOrDefault<string>("table_name");
            var mappingName = reader.GetValueOrDefault<string>("mapping_name");
            list.Add((tableName, mappingName));
        }
        return list;
    }

    private object? DeserializeSpecificSettings(ColumnDataType dataType, JObject jObject)
    {
        return dataType switch
        {
            ColumnDataType.Text => jObject.ToObject<TextColumnSettings>(),
            ColumnDataType.Number => jObject.ToObject<NumberColumnSettings>(),
            ColumnDataType.Boolean => jObject.ToObject<BooleanColumnSettings>(),
            ColumnDataType.Date => jObject.ToObject<DateColumnSettings>(),
            ColumnDataType.Currency => jObject.ToObject<CurrencyColumnSettings>(),
            ColumnDataType.List => jObject.ToObject<ListColumnSettings>(),
            ColumnDataType.MultilineText => jObject.ToObject<MultilineTextColumnSettings>(),
            _ => null
        };
    }

    public async Task<List<Dictionary<string, object>>> GetDataForEquipmentsAsync(List<int> tableIds)
    {
        var rows = new List<Dictionary<string, object>>();
        await using var connection = await _context.OpenNewConnectionAsync();
        const string sql = @"SELECT ""data"" FROM ""public"".""custom_data"" WHERE ""table_id"" = @tableId;";
        foreach (var id in tableIds)
        {
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@tableId", id);
            await using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var jsonString = reader.GetString(0);
                    var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);
                    if (dict != null)
                    {
                        rows.Add(dict);
                    }
                }
            }
        }
        return rows;
    }

    public async Task<List<int>> GetEquipmentSelectedColumnsIds(int summaryId)
    {
        List<int> columnIds = new List<int>();
        await using var connection = await _context.OpenNewConnectionAsync();
        const string sql = @"SELECT ""column_id"" FROM ""public"".""equipment_summary"" WHERE ""summary_id"" = @summaryId;";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@summaryId", summaryId);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            columnIds.Add(reader.GetValueOrDefault<int>("column_id"));
        }
        return columnIds;
    }

    public async Task DeleteEquipmentSummaryColumnAsync(List<int> columnIdsForDelete, int summaryId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            const string sql = @"DELETE FROM ""public"".""equipment_summary"" WHERE ""summary_id"" = @summaryId AND ""column_id"" = ANY(@columnIds);";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@summaryId", summaryId);
            cmd.Parameters.AddWithValue("@columnIds", columnIdsForDelete);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (NpgsqlException e)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task InsertEquipmentSummaryColumnAsync (List<int> columnIdIdsForInsert, int summaryId)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            const string sql = @"INSERT INTO ""public"".""equipment_summary"" (""summary_id"", ""column_id"") VALUES (@summaryId, @columnId);";
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@summaryId", summaryId);
            var pColumn = cmd.Parameters.Add("@columnId", NpgsqlTypes.NpgsqlDbType.Integer);
            foreach (var colId in columnIdIdsForInsert)
            {
                pColumn.Value = colId;
                await cmd.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        catch (NpgsqlException)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}