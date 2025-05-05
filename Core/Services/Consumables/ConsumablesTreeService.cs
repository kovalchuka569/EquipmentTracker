using System.Collections.ObjectModel;
using Common.Logging;
using Core.Models.Consumables;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using DbContext = Data.AppDbContext.DbContext;

namespace Core.Services.Consumables
{
    public class ConsumablesTreeService : IConsumablesTreeService
    {
        private IAppLogger<ConsumablesTreeService> _logger;
        private DbContext _context;
        public ConsumablesTreeService(IAppLogger<ConsumablesTreeService> logger, DbContext context)
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

        #region GetFoldersAsync
        public async Task<List<Folder>> GetFoldersAsync()
        {
            var folders = new List<Folder>();
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = "SELECT * FROM \"public\".\"ConsumablesFolders\"";
                using var cmd = new NpgsqlCommand(sql, connection);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        
                        folders.Add(new Folder
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            ParentId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2)
                        });
                    }
                }
                return folders;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting folders.");
                throw;
            }
            
        }
        #endregion
        #region GetFilesAsync
        public async Task<List<File>> GetFilesAsync()
        {
            var files = new List<File>();
            try
            {
                using var connection = await OpenNewConnectionAsync();
                string sql = "SELECT * FROM \"public\".\"ConsumablesFiles\"";
                using var cmd = new NpgsqlCommand(sql, connection);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        files.Add(new File
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1),
                            ParentIdFolder = reader.GetInt32(2)
                        });
                    }
                }
                return files;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error getting files.");
                throw;
            }
        }
        #endregion

        public ObservableCollection<IFileSystemItem> BuildHierachy(List<Folder> allFolders, List<File> allFiles)
        {
            var fodlerDict = allFolders.ToDictionary(f=> f.Id);
            
            // Adding files in folders
            foreach (var file in allFiles)
            {
                if (fodlerDict.TryGetValue(file.ParentIdFolder, out var parentFolder))
                {
                    parentFolder.AddFile(file);
                }
            }

            // Adding SubFolders
            foreach (var folder in allFolders)
            {
                if (folder.ParentId.HasValue && fodlerDict.TryGetValue(folder.ParentId.Value, out var parentFolder))
                {
                    parentFolder.AddFolder(folder);
                }
            }
            return new ObservableCollection<IFileSystemItem>(allFolders.Where(f => !f.ParentId.HasValue));
        }

        public async Task<int> InsertFolderAsync(Folder folder)
        {
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = "INSERT INTO \"ConsumablesFolders\" (\"Name\", \"ParentId\") VALUES (@name, @parentId) RETURNING \"id\";";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("name", folder.Name);
                cmd.Parameters.AddWithValue("parentId", (object)folder.ParentId ?? DBNull.Value);
                
                var newId = (int)await cmd.ExecuteScalarAsync();
                folder.Id = newId;
                return newId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inserting folder.");
                throw;
            }
        }

        public async Task<int> InsertFileAsync(File file)
        {
            string consumablesTableName = file.Name;
            string transactionsTableName = $"{consumablesTableName} операції";
            var consumablesColumns = new List<string>
            {
                "\"Назва\" VARCHAR(255)",
                "\"Категорія\" VARCHAR(255)",
                "\"Одиниця\" VARCHAR(255)",
                "\"Залишок\" NUMERIC(10, 2) DEFAULT 0",
                "\"Ціна за одиницю (грн)\" NUMERIC(10, 2)",
                "\"Мінімальний залишок\" NUMERIC(10, 2)",
                "\"Максимальний залишок\" NUMERIC(10, 2)", 
                "\"Дата, час останньої зміни\" TIMESTAMP",
                "\"Примітки\" TEXT"
            };
            var transactionsColumns = new List<string>
            {
                "\"Матеріал\" INTEGER",
                "\"Кількість\" NUMERIC(10, 2)",
                "\"Тип операції\" VARCHAR(255)",
                "\"Дата, час\" TIMESTAMP",
                "\"Опис\" TEXT",
                "\"Користувач\" INTEGER",
                $"FOREIGN KEY (\"Матеріал\") REFERENCES \"ConsumablesSchema\".\"{consumablesTableName}\" (\"id\")"
            };
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql =
                    "INSERT INTO \"ConsumablesFiles\" (\"Name\", \"FolderId\") VALUES (@name, @folderId) RETURNING \"id\"; " +
                    $"CREATE TABLE IF NOT EXISTS \"ConsumablesSchema\".\"{consumablesTableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", consumablesColumns)}); " +
                    $"CREATE TABLE IF NOT EXISTS \"ConsumablesSchema\".\"{transactionsTableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", transactionsColumns)}); ";
                
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("name", file.Name);
                cmd.Parameters.AddWithValue("folderId", file.ParentIdFolder);
                
                var newId = (int)await cmd.ExecuteScalarAsync();
                file.Id = newId;
                return newId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inserting file.");
                throw;
            }
        }

        public async Task<string> GenerateUniqueFolderNameAsync(string baseFolderName, int? folderId)
        {
            await using var connection = await OpenNewConnectionAsync();
            string sql = "SELECT \"Name\" FROM \"ConsumablesFolders\"";
            using var cmd = new NpgsqlCommand(sql, connection);

            var existingNames = new HashSet<string>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingNames.Add(reader.GetString(0));
                }
            }

            if (!existingNames.Contains(baseFolderName)) return baseFolderName;

            int counter = 1;
            string newName;
            do
            {
                newName = $"{baseFolderName} ({counter})";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }

        public async Task<string> GenerateUniqueFileNameAsync(string baseFileName, int parentFolderId)
        {
            await using var connection = await OpenNewConnectionAsync();
            string sql = "SELECT \"Name\" FROM \"ConsumablesFiles\"";
            using var cmd = new NpgsqlCommand(sql, connection);

            var existingNames = new HashSet<string>();
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    existingNames.Add(reader.GetString(0));
                }
            }

            if (!existingNames.Contains(baseFileName)) return baseFileName;

            int counter = 1;
            string newName;
            do
            {
                newName = $"{baseFileName} ({counter})";
                counter++;
            } while (existingNames.Contains(newName));

            return newName;
        }

        public async Task RenameFolderAsync(string newName, int folderId)
        {
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = "UPDATE \"ConsumablesFolders\" SET \"Name\" = @name WHERE \"id\" = @folderId";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("name", newName);
                cmd.Parameters.AddWithValue("folderId", folderId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error renameing folder");
                throw;
            }
        }

        public async Task RenameFileAsync(string newName, string oldFileName, int fileId)
        {
            string oldTransactionsTableName = $"{oldFileName} операції";
            string newTransactionsTableName = $"{newName} операції";
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = "UPDATE \"ConsumablesFiles\" SET \"Name\" = @name WHERE \"id\" = @fileId; " +
                             $"ALTER TABLE \"ConsumablesSchema\".\"{oldFileName}\" RENAME TO \"{newName}\"; " +
                             $"ALTER TABLE \"ConsumablesSchema\".\"{oldTransactionsTableName}\" RENAME TO \"{newTransactionsTableName}\";";
                using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("name", newName);
                cmd.Parameters.AddWithValue("fileId", fileId);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error renameing file");
                throw;
            }
        }
    }
}
