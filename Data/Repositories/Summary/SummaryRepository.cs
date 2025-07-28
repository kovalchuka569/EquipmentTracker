using Common.Logging;
using Data.ApplicationDbContext;
using Models.Equipment;
using Models.Equipment.ColumnSettings;
using Models.Equipment.ColumnSpecificSettings;
using Models.EquipmentTree;
using Models.Summary.DataGrid;
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
    private readonly AppDbContext _context;

    public SummaryRepository(IAppLogger<SummaryRepository> logger, AppDbContext context)
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
        string sql = @"SELECT * FROM ""public"".""folders"" WHERE ""deleted"" = false";
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
        string sql = @"SELECT * FROM ""public"".""files"" 
                       WHERE ""file_format"" = 0 
                       AND ""deleted"" = false";
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
        string sql = @"SELECT settings->>'HeaderText' AS ""header_text"", ""id"" FROM ""public"".""custom_columns""
                       WHERE ""table_id"" = @tableId 
                       AND ""deleted"" = false;";
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

    public async Task InsertEquipmentSummaryColumnAsync (List<int> columnIdsForInsert, int summaryId, CancellationToken ct = default)
    {
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            var values = string.Join(", ", columnIdsForInsert.Select((_, i) => $"(@summaryId, @col{i})"));

            var sql = $@"INSERT INTO ""public"".""equipment_summary"" (""summary_id"", ""column_id"") VALUES {values};";
            
            using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@summaryId", summaryId);
            for (var i = 0; i < columnIdsForInsert.Count; i++)
            {
                cmd.Parameters.AddWithValue($"@col{i}", columnIdsForInsert[i]);
            }
            await cmd.ExecuteNonQueryAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (NpgsqlException)
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<List<ReportColumnMetadata>> GetEquipmentReportColumnsMetadata(int summaryId, CancellationToken ct = default)
    {
        var result = new List<ReportColumnMetadata>();
        await using var connection = await _context.OpenNewConnectionAsync(ct);
        const string sql = @"SELECT es.""column_id"" AS ""custom_column_id"", 
                                    es.""merged_with"" AS ""merged_with"", 
                                    es.""user_accepted_merge"" AS ""user_accepted_merge"", 
                                    es.""is_merge_decision_made"" AS ""is_merge_decision_made"",
                                    cc.""settings"" AS ""column_settings"",
                                    cc.""settings"" ->> 'HeaderText' AS ""header_text"", 
                                    cc.""settings"" ->> 'MappingName' AS ""mapping_name"",
                                    f.""name"" AS ""equipment_sheet_name"", 
                                    ct.""id"" AS ""table_id"", 
                                    es.""id"" AS ""equipment_summary_entry_id"" 
                            FROM ""public"".""equipment_summary"" es    
                            JOIN ""public"".""custom_columns"" cc ON es.""column_id"" = cc.""id"" 
                            JOIN ""public"".""custom_tables"" ct ON cc.""table_id"" = ct.""id"" 
                            JOIN ""public"".""files"" f ON ct.""id"" = f.""table_id"" 
                            WHERE es.""summary_id"" = @summaryId 
                            AND cc.""deleted"" = false";  
        await using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@summaryId", summaryId);
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var reportColumnMetadata = new ReportColumnMetadata();
            reportColumnMetadata.CustomColumnId = reader.GetValueOrDefault<int>("custom_column_id");
            reportColumnMetadata.MergedIntoColumnId = reader.GetValueOrDefault<int?>("merged_with");
            reportColumnMetadata.UserAcceptedMerge = reader.GetValueOrDefault<bool>("user_accepted_merge");
            reportColumnMetadata.IsMergeDecisionMade = reader.GetValueOrDefault<bool?>("is_merge_decision_made");
            
            var columnSettings = JsonConvert.DeserializeObject<ColumnSettingsDisplayModel>(reader.GetValueOrDefault<string>("column_settings"));
            if (columnSettings != null && columnSettings.SpecificSettings is JObject jObject)
            {
                columnSettings.SpecificSettings = DeserializeSpecificSettings(columnSettings.DataType, jObject);
            }
            reportColumnMetadata.ColumnSettings = columnSettings;

            
            reportColumnMetadata.HeaderText = reader.GetValueOrDefault<string>("header_text");
            reportColumnMetadata.MappingName = reader.GetValueOrDefault<string>("mapping_name");
            reportColumnMetadata.EquipmentSheetName = reader.GetValueOrDefault<string>("equipment_sheet_name");
            reportColumnMetadata.TableId = reader.GetValueOrDefault<int>("table_id");
            reportColumnMetadata.EquipmentSummaryEntryId = reader.GetValueOrDefault<int>("equipment_summary_entry_id");
            result.Add(reportColumnMetadata);
        }

        return result;
    }

    public async Task UpdateEquipmentSummaryMergedStatus(int equipmentSummaryEntryId, int? mergedWith, bool userAcceptedMerge, bool isMergedDecisionMade)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string sql = @"UPDATE ""public"".""equipment_summary"" SET ""merged_with"" = @mergedWith, ""is_merge_decision_made"" = @isMergedDecisionMade, ""user_accepted_merge"" = @userAcceptedMerge WHERE ""id"" = @equipmentSummaryEntryId;";
            await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            cmd.Parameters.AddWithValue("@mergedWith", mergedWith ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@isMergedDecisionMade", isMergedDecisionMade);
            cmd.Parameters.AddWithValue("@userAcceptedMerge", userAcceptedMerge);
            cmd.Parameters.AddWithValue("@equipmentSummaryEntryId", equipmentSummaryEntryId);
            await cmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<RawDataEntry>> GetRawDataForEquipmentsAsync(List<int> tableIds, CancellationToken ct = default)
    {
        await using var connection = await _context.OpenNewConnectionAsync();
        const string sql = @"SELECT 
                             r.""id"" AS ""row_id"", 
                             jsonb_object_agg(col.""settings"" ->> 'MappingName', c.""value"") AS ""row_data_jsonb"" 
                             FROM ""public"".""equipment_rows"" r 
                             JOIN ""public"".""equipment_cells"" c ON r.""id"" = c.""row_id"" 
                             JOIN ""public"".""custom_columns"" col ON c.""column_id"" = col.""id"" 
                             WHERE r.""table_id"" = ANY(@tableIds) 
                             AND col.""deleted"" = false 
                             AND r.""deleted"" = false
                             AND c.""deleted"" = false
                             GROUP BY r.""id"" 
                             ORDER BY r.""position"";";
        using var cmd = new NpgsqlCommand(sql, connection);
        cmd.Parameters.AddWithValue("@tableIds", tableIds);

        var result = new List<RawDataEntry>();
        
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            result.Add(new RawDataEntry
            {
                RowId = reader.GetValueOrDefault<int>("row_id"),
                RowDataJsonb = reader.GetValueOrDefault<string>("row_data_jsonb") 
            });
        }

        return result;
    }
    
    private static object? DeserializeSpecificSettings(ColumnDataType dataType, JObject jObject)
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
}