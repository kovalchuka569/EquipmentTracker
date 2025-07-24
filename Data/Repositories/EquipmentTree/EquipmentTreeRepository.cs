using Common.Logging;
using Data.ApplicationDbContext;
using Models.Equipment;
using Models.EquipmentTree;
using Models.NavDrawer;
using Npgsql;
using NpgsqlTypes;

namespace Data.Repositories.EquipmentTree
{
    public class EquipmentTreeRepository : IEquipmentTreeRepository
    {
        private readonly IAppLogger<EquipmentTreeRepository> _logger;
        private readonly AppDbContext _context;
        
        public EquipmentTreeRepository(IAppLogger<EquipmentTreeRepository> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        
        public async Task<List<FolderDto>> GetFoldersAsync(MenuType menuType)
        {
            var folders = new List<FolderDto>();
            try
            {
                await using var connection = await _context.OpenNewConnectionAsync();
                const string sql = @"SELECT * FROM ""public"".""folders"" 
                                     WHERE ""menu_type"" = @menuType 
                                     AND ""deleted"" = false";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@menuType", (int)menuType);
                await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        folders.Add(new FolderDto
                        {
                            Id = reader.GetValueOrDefault<int>("id"),
                            Name = reader.GetValueOrDefault<string>("name"),
                            ParentId = reader.GetValueOrDefault<int>("parent_id"),
                            MenuType = reader.GetValueOrDefault<MenuType>("menu_type")
                        });
                    }
                }
                return folders;
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e.Message, "Database error getting folders");
                throw;
            }
        }

        public async Task<List<FileDto>> GetFilesAsync(MenuType menuType)
        {
            var files = new List<FileDto>();
            try
            {
                await using var connection = await _context.OpenNewConnectionAsync();
                string sql = @"SELECT * FROM ""public"".""files"" 
                               WHERE ""menu_type"" = @menuType 
                               AND ""deleted"" = false";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@menuType", (int)menuType);
               await using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        files.Add(new FileDto
                        {
                            Id = reader.GetValueOrDefault<int>("id"),
                            Name = reader.GetValueOrDefault<string>("name"),
                            FolderId = reader.GetValueOrDefault<int>("folder_id"),
                            FileFormat = reader.GetValueOrDefault<FileFormat>("file_format"),
                            SummaryId = reader.GetValueOrDefault<int>("summary_id"),
                            TableId = reader.GetValueOrDefault<int>("table_id")
                        });
                    }
                }
                return files;
            }
            catch (NpgsqlException e)
            {   
                _logger.LogError(e.Message, "Database error getting files");
                throw;
            }
        }

        public async Task<int> CreateFolderAsync(string name, int? parentId, MenuType menuType)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = @"INSERT INTO ""public"".""folders"" (""name"", ""parent_id"", ""menu_type"") VALUES (@name, @parentId, @menuType) RETURNING ""id"";";
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@parentId", parentId);
                cmd.Parameters.AddWithValue("@menuType", (int)menuType);
                var newId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return newId;
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error inserting folder");
                throw;
            }
        }

        public async Task<int> CreateEquipmentTableAsync()
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = @"INSERT INTO ""public"".""custom_tables"" (""code"") VALUES (@code) RETURNING ""id"";";
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@code", NpgsqlDbType.Uuid, Guid.NewGuid());
                var newId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return newId;
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error creating table");
                throw;
            }
        }

        public async Task<int> CreateSummaryAsync(SummaryFormat summaryFormat)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = @"INSERT INTO ""public"".""summaries"" (""summary_format"") VALUES (@summaryFormat) RETURNING ""id"";";
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@summaryFormat", (int)summaryFormat);
                var newId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return newId;
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error creating summary");
                throw;
            }
        }
        
        public async Task<int> CreateSummaryFileAsync(string name, int folderId, int summaryId, MenuType menuType)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sqlCreateTable = @"INSERT INTO ""public"".""files"" (""name"", ""file_format"", ""folder_id"", ""summary_id"", ""menu_type"") VALUES (@name, @fileFormat, @folderId, @summaryId, @menuType) RETURNING ""id"";";
                using var cmd = new NpgsqlCommand(sqlCreateTable, connection, transaction);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@fileFormat", (int)FileFormat.Summary);
                cmd.Parameters.AddWithValue("@folderId", folderId);
                cmd.Parameters.AddWithValue("@summaryId", summaryId);
                cmd.Parameters.AddWithValue("@menuType", (int)menuType);
                var newId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return newId;
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error inserting summary file");
                throw;
            }
        }

        public async Task<int> CreateFileAsync(string name, FileFormat fileFormat, int folderId, int tableId, MenuType menuType)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sqlCreateTable = @"INSERT INTO ""public"".""files"" (""name"", ""file_format"", ""folder_id"", ""table_id"", ""menu_type"") VALUES (@name, @fileFormat, @folderId, @tableId, @menuType) RETURNING ""id"";";
                using var cmd = new NpgsqlCommand(sqlCreateTable, connection, transaction);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@fileFormat", (int)fileFormat);
                cmd.Parameters.AddWithValue("@folderId", folderId);
                cmd.Parameters.AddWithValue("@tableId", tableId);
                cmd.Parameters.AddWithValue("@menuType", (int)menuType);
                var newId = (int)await cmd.ExecuteScalarAsync();
                await transaction.CommitAsync();
                return newId;
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error inserting file");
                throw;
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error inserting file");
                throw;
            }
        }

        public async Task RenameFolderAsync(int folderId, string newName)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = @"UPDATE ""public"".""folders"" SET ""name"" = @name WHERE ""id"" = @id";
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", folderId);
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error renaming folder");
                throw;
            }
        }
        
        public async Task RenameFileAsync(int fileId, string newName)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = @"UPDATE ""public"".""files"" SET ""name"" = @name WHERE ""id"" = @id";
                await using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", fileId);
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch (NpgsqlException e)
            {
                await transaction.RollbackAsync();
                _logger.LogError(e.Message, "Database error renaming file");
                throw;
            }
        }
        
    }
}
