using Microsoft.EntityFrameworkCore;

using Npgsql;

using Models.Entities.EquipmentSheet;
using Models.Entities.PivotSheet;

using Data.Entities;
using Data.Configurations;
using Models.Entities.FileSystem;

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
        
        public DbSet<User> Users { get; set; }
        

        public DbSet<FileSystemItemEntity> FileSystemItems { get; set; }
        
        public DbSet<PivotSheetEntity> PivotSheets { get; set; }
        
        public DbSet<EquipmentSheetEntity> EquipmentSheets { get; set; }
    }
}
