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
            var folderDict = allFolders.ToDictionary(f => f.Id);

            // Adding files in folders
            foreach (var file in allFiles)
            {
                if (folderDict.TryGetValue(file.ParentIdFolder, out var parentFolder))
                {
                    parentFolder.AddFile(file);
                }
            }

            // Adding SubFolders
            foreach (var folder in allFolders)
            {
                if (folder.ParentId.HasValue && folderDict.TryGetValue(folder.ParentId.Value, out var parentFolder))
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
                string sql =
                    "INSERT INTO \"ConsumablesFolders\" (\"Name\", \"ParentId\") VALUES (@name, @parentId) RETURNING \"id\";";
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
                "\"Залишок\" NUMERIC(10, 2) DEFAULT 0.00",
                "\"Мінімальний залишок\" NUMERIC(10, 2) DEFAULT 0.00",
                "\"Максимальний залишок\" NUMERIC(10, 2) DEFAULT 0.00",
                "\"Дата, час останньої зміни\" TIMESTAMP",
                "\"Примітки\" TEXT"
            };
            var transactionsColumns = new List<string>
            {
                "\"Матеріал\" INTEGER",
                "\"Кількість\" NUMERIC(10, 2)",
                "\"Залишок після\" NUMERIC(10, 2)",
                "\"Тип операції\" VARCHAR(255)",
                "\"Дата, час\" TIMESTAMP",
                "\"Квитанція\" BYTEA",
                "\"Опис\" TEXT",
                "\"Користувач\" INTEGER",
                $"FOREIGN KEY (\"Матеріал\") REFERENCES \"ConsumablesSchema\".\"{consumablesTableName}\" (\"id\")"
            };
            try
            {
                await using var connection = await OpenNewConnectionAsync();

                await using var transaction = await connection.BeginTransactionAsync();

                string sqlInsertFile =
                    "INSERT INTO \"ConsumablesFiles\" (\"Name\", \"FolderId\") VALUES (@name, @folderId) RETURNING \"id\";";
                await using var cmdInsertFile = new NpgsqlCommand(sqlInsertFile, connection, transaction);
                cmdInsertFile.Parameters.AddWithValue("name", file.Name);
                cmdInsertFile.Parameters.AddWithValue("folderId", file.ParentIdFolder);

                var newId = (int)await cmdInsertFile.ExecuteScalarAsync();
                file.Id = newId;

                string sqlCreateTableConsumables =
                    $"CREATE TABLE IF NOT EXISTS \"ConsumablesSchema\".\"{consumablesTableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", consumablesColumns)});";
                await using var cmdCreateTableConsumables =
                    new NpgsqlCommand(sqlCreateTableConsumables, connection, transaction);
                await cmdCreateTableConsumables.ExecuteNonQueryAsync();

                string sqlCreateTableTransactions =
                    $"CREATE TABLE IF NOT EXISTS \"ConsumablesSchema\".\"{transactionsTableName}\" (Id SERIAL PRIMARY KEY, {string.Join(", ", transactionsColumns)});";
                await using var cmdCreateTableTransactions =
                    new NpgsqlCommand(sqlCreateTableTransactions, connection, transaction);
                await cmdCreateTableTransactions.ExecuteNonQueryAsync();

                string sqlCreateFunctionIfNotExists = $@"
                CREATE OR REPLACE FUNCTION ConsumablesSchema.count_low_stock_{consumablesTableName}()
                RETURNS INTEGER AS $$
                DECLARE
                    low_count INTEGER;
                BEGIN
                    SELECT COUNT(*) INTO low_count
                    FROM ConsumablesSchema.""{consumablesTableName}""
                    WHERE ""Залишок"" < ""Мінімальний залишок"";
                    RETURN low_count;
                END;
                $$ LANGUAGE plpgsql;
                ";

                await using var cmdCreateFunctionIfNotExists =
                    new NpgsqlCommand(sqlCreateFunctionIfNotExists, connection, transaction);
                await cmdCreateFunctionIfNotExists.ExecuteNonQueryAsync();

                string sqlCreateTrigger = $@"
                CREATE OR REPLACE TRIGGER update_low_stock_count_{consumablesTableName}
                AFTER INSERT OR UPDATE OR DELETE ON ConsumablesSchema.""{consumablesTableName}""
                FOR EACH ROW
                EXECUTE FUNCTION ConsumablesSchema.count_low_stock_{consumablesTableName}();
                ";

                await using var cmdCreateTrigger = new NpgsqlCommand(sqlCreateTrigger, connection, transaction);
                await cmdCreateTrigger.ExecuteNonQueryAsync();

                await transaction.CommitAsync();
                return newId;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error inserting file and creating tables/trigger.");
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

        public async Task<Dictionary<string, int>> GetLowValueCountsAsync(List<string> tableNames)
        {
            var lowValues = new Dictionary<string, int>();
            try
            {
                await using var connection = await OpenNewConnectionAsync();
                string sql = "SELECT table_name, low_value_count FROM \"ConsumablesSchema\".get_low_value_counts(@tableNames)";
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("tableNames", tableNames.ToArray());
                
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    string tableName = reader.GetString(0);
                    long count = reader.GetInt64(1);
                    lowValues[tableName] = (int)count;
                }
                return lowValues;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    
}
}
