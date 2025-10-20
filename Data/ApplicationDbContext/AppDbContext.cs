using Microsoft.EntityFrameworkCore;
using Npgsql;
using Data.Entities;
using Data.Entities.MainTree;

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
            
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }
        
        public async Task<NpgsqlConnection> OpenNewConnectionAsync(CancellationToken ct = default)
        { 
            var connectionString = Database.GetDbConnection().ConnectionString;
            var connection = new NpgsqlConnection(connectionString);
            await connection.OpenAsync(ct);
            return connection;
        }
        
        public DbSet<UserEntity> Users { get; set; }
        public DbSet<MainTreeItemEntity> MainTreeItems { get; set; }
        public DbSet<PivotSheetEntity> PivotSheets { get; set; }
        public DbSet<EquipmentSheetEntity> EquipmentSheets { get; set; }
    }
}
