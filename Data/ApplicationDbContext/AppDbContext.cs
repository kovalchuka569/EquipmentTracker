using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Data.Configurations;
using Models.Entities.EquipmentSheet;
using Models.Entities.FileSystem;
using Models.Entities.SummarySheet;
using Npgsql;

namespace Data.ApplicationDbContext
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new FolderConfiguration());
            modelBuilder.ApplyConfiguration(new FileConfiguration());
            modelBuilder.ApplyConfiguration(new EquipmentSheetConfigurations());
            modelBuilder.ApplyConfiguration(new SummaryFileConfiguration());
        }
        
        public async Task<NpgsqlConnection> OpenNewConnectionAsync(CancellationToken ct = default)
        { 
            var connectionString = Database.GetDbConnection().ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(ct);
            return connection;
        }

        public async Task BeginTransactionAsync(CancellationToken ct)
        {
            if (Database.CurrentTransaction == null)
            {
                await Database.BeginTransactionAsync(ct);
            }
        }

        public async Task CommitTransactionAsync(CancellationToken ct)
        {
            if (Database.CurrentTransaction != null)
            {
               await Database.CurrentTransaction.CommitAsync(ct);
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken ct)
        {
            if (Database.CurrentTransaction != null)
            {
               await Database.CurrentTransaction.RollbackAsync(ct);
            }
        }



        public DbSet<User> Users { get; set; }

        #region File folders

        public DbSet<FileEntity> Files { get; set; }
        public DbSet<FolderEntity> Folders { get; set; }

        #endregion
        
        

        public DbSet<SummarySheetEntity> SummarySheets { get; set; }
        public DbSet<EquipmentSheetEntity> EquipmentSheets { get; set; }
    }
}
