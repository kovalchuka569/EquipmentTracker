using Common.Logging;
using Models.EquipmentTree;
using Npgsql;
using DbContext = Data.AppDbContext.DbContext;

namespace Data.Repositories.EquipmentTree
{
    public class EquipmentTreeRepository : IEquipmentTreeRepository
    {
        private readonly IAppLogger<EquipmentTreeRepository> _logger;
        private readonly DbContext _context;
        
        public EquipmentTreeRepository(IAppLogger<EquipmentTreeRepository> logger, DbContext context)
        {
            _logger = logger;
            _context = context;
        }
        
        
        public async Task<List<FolderDto>> GetFoldersAsync(string menuType)
        {
            var folders = new List<FolderDto>();
            try
            {
                await using var connection = await _context.OpenNewConnectionAsync();
                string sql = "SELECT * FROM \"public\".\"EquipmentTreeFolders\" WHERE \"MenuType\" = @menuType";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@menuType", menuType);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        folders.Add(new FolderDto
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            ParentId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                            MenuType = reader.GetString(3)
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

        public async Task<List<FileDto>> GetFilesAsync(string menuType)
        {
            var files = new List<FileDto>();
            try
            {
                using var connection = await _context.OpenNewConnectionAsync();
                string sql = "SELECT * FROM \"public\".\"EquipmentTreeFiles\" WHERE \"MenuType\" = @menuType";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("@menuType", menuType);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        files.Add(new FileDto
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            FolderId = reader.GetInt32(2),
                            TableName = reader.GetString(4),
                            FileType = reader.GetString(5),
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

        public async Task<int> InsertFolderAsync(string name, int? parentId, string menuType)
        {
            using var connection = await _context.OpenNewConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql = "INSERT INTO \"public\".\"EquipmentTreeFolders\" (\"Name\", \"ParentId\", \"MenuType\") VALUES (@name, @parentId, @menuType) RETURNING \"id\";";
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@parentId", parentId);
                cmd.Parameters.AddWithValue("@menuType", menuType);
                var newId = (int)await cmd.ExecuteScalarAsync();
                transaction.Commit();
                return newId;
            }
            catch (NpgsqlException e)
            {
                transaction.Rollback();
                _logger.LogError(e.Message, "Database error inserting folder");
                throw;
            }
        }

        public async Task<int> InsertFileAsync(string name, int folderId, string menuType)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql =
                    "INSERT INTO \"public\".\"EquipmentTreeFiles\" (\"Name\", \"FolderId\", \"MenuType\", \"TableName\", \"FileType\") VALUES (@name, @folderId, @menuType, @tableName, @fileType) RETURNING \"id\";";
                using var cmd1 = new NpgsqlCommand(sql, connection, transaction);
                cmd1.Parameters.AddWithValue("@name", name);
                cmd1.Parameters.AddWithValue("@folderId", folderId);
                cmd1.Parameters.AddWithValue("@menuType", menuType);
                cmd1.Parameters.AddWithValue("@tableName", name);
                cmd1.Parameters.AddWithValue("@fileType", "equipments table");
                var newId = (int)await cmd1.ExecuteScalarAsync();
                Console.WriteLine(sql);

                string sqlInsertingServiceFolder =
                    "INSERT INTO \"public\".\"EquipmentTreeFolders\" (\"Name\", \"ParentId\", \"MenuType\") VALUES (@name, @parentId, @menuType) RETURNING \"id\"; ";
                using var cmd2 = new NpgsqlCommand(sqlInsertingServiceFolder, connection, transaction);
                cmd2.Parameters.AddWithValue("@name", $"{name} технічні роботи");
                cmd2.Parameters.AddWithValue("@parentId", folderId);
                cmd2.Parameters.AddWithValue("@menuType", menuType);
                var serviceFolderId = (int)await cmd2.ExecuteScalarAsync();
                Console.WriteLine(sqlInsertingServiceFolder);

                string sqlInsertingWriteOff =
                    "INSERT INTO \"public\".\"EquipmentTreeFiles\" (\"Name\", \"FolderId\", \"MenuType\", \"TableName\", \"FileType\") VALUES (@name, @folderId, @menuType, @tableName, @fileType); ";
                using var cmd3 = new NpgsqlCommand(sqlInsertingWriteOff, connection, transaction);
                cmd3.Parameters.AddWithValue("@name", $"{name} списані");
                cmd3.Parameters.AddWithValue("@folderId", folderId);
                cmd3.Parameters.AddWithValue("@menuType", menuType);
                cmd3.Parameters.AddWithValue("@tableName", name);
                cmd3.Parameters.AddWithValue("@fileType", "writeoff");
                await cmd3.ExecuteScalarAsync();
                Console.WriteLine(sqlInsertingWriteOff);

                string sqlInsertingServiceFile =
                    "INSERT INTO \"public\".\"EquipmentTreeFiles\" (\"Name\", \"FolderId\", \"MenuType\", \"TableName\", \"FileType\") VALUES (@name, @folderId, @menuType, @tableName, @fileType); ";
                using var cmd4 = new NpgsqlCommand(sqlInsertingServiceFile, connection, transaction);
                cmd4.Parameters.AddWithValue("@name", $"{name} обслуговування");
                cmd4.Parameters.AddWithValue("@folderId", serviceFolderId);
                cmd4.Parameters.AddWithValue("@menuType", menuType);
                cmd4.Parameters.AddWithValue("@tableName", $"{name} О");
                cmd4.Parameters.AddWithValue("@fileType", "services");
                await cmd4.ExecuteScalarAsync();
                Console.WriteLine(sqlInsertingServiceFile);

                string sqlInsertingRepairsFile =
                    "INSERT INTO \"public\".\"EquipmentTreeFiles\" (\"Name\", \"FolderId\", \"MenuType\", \"TableName\", \"FileType\") VALUES (@name, @folderId, @menuType, @tableName, @fileType); ";
                using var cmd5 = new NpgsqlCommand(sqlInsertingRepairsFile, connection, transaction);
                cmd5.Parameters.AddWithValue("@name", $"{name} ремонти");
                cmd5.Parameters.AddWithValue("@folderId", serviceFolderId);
                cmd5.Parameters.AddWithValue("@menuType", menuType);
                cmd5.Parameters.AddWithValue("@tableName", $"{name} Р");
                cmd5.Parameters.AddWithValue("@fileType", "repairs");
                await cmd5.ExecuteScalarAsync();
                Console.WriteLine(sqlInsertingRepairsFile);
                
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
                string sql = "UPDATE \"public\".\"EquipmentTreeFolders\" SET \"Name\" = @name WHERE \"id\" = @id";
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@name", newName);
                cmd.Parameters.AddWithValue("@id", folderId);
                await cmd.ExecuteNonQueryAsync();
                transaction.Commit();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                _logger.LogError(e.Message, "Database error renaming folder");
                throw;
            }
        }

        public async Task RenameChildsAsync(int folderId, string newName, string oldName, string menuType)
        {
            await using var connection = await _context.OpenNewConnectionAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                string sql =
                    "UPDATE \"public\".\"EquipmentTreeFiles\" SET \"Name\" = @newName, \"TableName\" = @newName WHERE \"Name\" = @oldName AND \"MenuType\" = @menuType; " +
                    "UPDATE \"public\".\"EquipmentTreeFolders\" SET \"Name\" = @newNameGeneralFolder WHERE \"Name\" = @oldNameGeneralFolder AND \"MenuType\" = @menuType; " +
                    "UPDATE \"public\".\"EquipmentTreeFiles\" SET \"Name\" = @newNameWriteOff, \"TableName\" = @newName WHERE \"Name\" = @oldNameWriteOff AND \"MenuType\" = @menuType; " +
                    "UPDATE \"public\".\"EquipmentTreeFiles\" SET \"Name\" = @newNameServices, \"TableName\" = @newTableNameServices WHERE \"Name\" = @oldNameServices AND \"MenuType\" = @menuType; " +
                    "UPDATE \"public\".\"EquipmentTreeFiles\" SET \"Name\" = @newNameRepairs, \"TableName\" = @newTableNameRepairs WHERE \"Name\" = @oldNameRepairs AND \"MenuType\" = @menuType; " +
                    $"ALTER TABLE \"UserTables\".\"{oldName}\" RENAME TO \"{newName}\"; " +
                    $"ALTER TABLE \"UserTables\".\"{oldName} О\" RENAME TO \"{newName} О\"; " + // "О" means services - "(О)бслуговування"
                    $"ALTER TABLE \"UserTables\".\"{oldName} ОВМ\" RENAME TO \"{newName} ОВМ\"; " + // "ОВМ" mean service consumables - "(О)бслуговування (В)итрачені (М)атеріали"
                    $"ALTER TABLE \"UserTables\".\"{oldName} Р\" RENAME TO \"{newName} Р\"; " + // "Р" means repairs - "(Р)емонти"
                    $"ALTER TABLE \"UserTables\".\"{oldName} РВМ\" RENAME TO \"{newName} РВМ\"; "; // "РВМ" mean repair consumables - "(Р)емонти (В)итрачені (М)атеріали"
                
                Console.WriteLine(sql);
                using var cmd = new NpgsqlCommand(sql, connection, transaction);
                cmd.Parameters.AddWithValue("@newName", newName);
                cmd.Parameters.AddWithValue("@newNameGeneralFolder", $"{newName} технічні роботи");
                cmd.Parameters.AddWithValue("@newNameWriteOff", $"{newName} списані");
                cmd.Parameters.AddWithValue("@newNameServices", $"{newName} обслуговування");
                cmd.Parameters.AddWithValue("@newTableNameServices", $"{newName} О");
                cmd.Parameters.AddWithValue("@newNameRepairs", $"{newName} ремонти");
                cmd.Parameters.AddWithValue("@newTableNameRepairs", $"{newName} Р");
                cmd.Parameters.AddWithValue("@oldName", oldName);
                cmd.Parameters.AddWithValue("@oldNameGeneralFolder", $"{oldName} технічні роботи");
                cmd.Parameters.AddWithValue("@oldNameWriteOff", $"{oldName} списані");
                cmd.Parameters.AddWithValue("@oldNameServices", $"{oldName} обслуговування");
                cmd.Parameters.AddWithValue("@oldNameRepairs", $"{oldName} ремонти");
                cmd.Parameters.AddWithValue("@menuType", menuType);
                await cmd.ExecuteNonQueryAsync();
                transaction.Commit();
                
                
            }
            catch (NpgsqlException e)
            {
                _logger.LogError(e.Message, "Database error renaming childs");
                throw;
            }
        }
    }
}
