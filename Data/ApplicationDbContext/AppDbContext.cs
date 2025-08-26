using Microsoft.EntityFrameworkCore;

using Npgsql;

using Models.Entities.EquipmentSheet;
using Models.Entities.PivotSheet;

using Data.Entities;
using Data.Configurations;
using Models.Entities.FileSystem;
using Models.FileSystem;

namespace Data.ApplicationDbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration(new FileSystemItemConfiguration());
            modelBuilder.ApplyConfiguration(new EquipmentSheetConfigurations());
            modelBuilder.ApplyConfiguration(new PivotSheetConfigurations());
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
        

        public DbSet<FileSystemItemEntity> FileSystemItems { get; set; }
        
        public DbSet<PivotSheetEntity> PivotSheets { get; set; }
        
        public DbSet<EquipmentSheetEntity> EquipmentSheets { get; set; }
    }
}
