using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Common.Logging;
using Npgsql;
using Syncfusion.PMML;
using TableColumn = Data.Entities.TableColumn;

namespace Data.AppDbContext
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext
    {
        private IAppLogger<DbContext> _logger;
        private readonly DbContextOptions<DbContext> _options;
        public DbContext(IAppLogger<DbContext> logger, DbContextOptions<DbContext> options) : base(options)
        {
            _logger = logger;
            _options = options;
            _logger.LogInformation("Created DbContext!");
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        
        ~DbContext()
        {
            _logger.LogInformation("Finalized DbContext!");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.Time).HasColumnType("interval");
            });
            modelBuilder.Entity<TableColumn>().HasNoKey().ToView(null);

        }
        
        public async Task<NpgsqlConnection> OpenNewConnectionAsync()
        {
            try
            {
                var connectionString = Database.GetDbConnection().ConnectionString;

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
        
        public DbContext Create()
        {
            return new DbContext(_logger, _options);
        }
        
        public DbSet<User> Users { get; set; }
        
        // Categories equipment
        public DbSet<CategoryProductionEquipment> CategoriesProductionEquipment { get; set; }
        public DbSet<CategoryFurniture> CategoriesFurniture { get; set; }
        public DbSet<CategoryOfficeTechnique> CategoriesOfficeTechnique { get; set; }
        public DbSet<CategoryTool> CategoriesTool { get; set; }
        
        public DbSet<FileEntity>Files { get; set; }
        
        public DbSet<Log> Logs { get; set; }
        
        
    }
}
