using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using Data.Configurations;
using Models.Entities.Table;
using Npgsql;

namespace Data.ApplicationDbContext
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.ApplyConfiguration(new TableConfiguration());
            modelBuilder.ApplyConfiguration(new RowConfiguration());
            modelBuilder.ApplyConfiguration(new ColumnConfiguration());
            modelBuilder.ApplyConfiguration(new CellConfiguration());
            
            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.Time).HasColumnType("interval");
            });
            modelBuilder.Entity<TableColumn>().HasNoKey().ToView(null);
            
        }
        
        public async Task<NpgsqlConnection> OpenNewConnectionAsync(CancellationToken ct = default)
        {
            try
            {
                var connectionString = Database.GetDbConnection().ConnectionString;
                var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync(ct);
                return connection;
            }
            catch (NpgsqlException ex)
            {
                throw;
            }
        }
        
        public DbSet<User> Users { get; set; }

        #region Table

        public DbSet<TableEntity> Tables { get; set; }
        public DbSet<ColumnEntity> Columns { get; set; }
        public DbSet<RowEntity> Rows { get; set; }
        public DbSet<CellEntity> Cells { get; set; }
        
        #endregion
    }
}
