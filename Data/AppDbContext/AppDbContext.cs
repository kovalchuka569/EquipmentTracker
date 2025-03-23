using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.PMML;

namespace Data.AppDbContext
{
    public class AppDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var dbHost = Properties.Settings.Default.DbHost;
            var dbName = Properties.Settings.Default.DbName;
            var dbUser = Properties.Settings.Default.DbUser;
            var dbPassword = Properties.Settings.Default.DbPassword;
            var connectionString = $"Host={dbHost};Database={dbName};Username={dbUser};Password={dbPassword}";
            optionsBuilder.UseNpgsql(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Log>(entity =>
            {
                entity.Property(e => e.Date).HasColumnType("date");
                entity.Property(e => e.Time).HasColumnType("interval");
            });
        }
        
        public DbSet<User> Users { get; set; }
        
        // Categories equipment
        public DbSet<CategoryProductionEquipment> CategoriesProductionEquipment { get; set; }
        public DbSet<CategoryFurniture> CategoriesFurniture { get; set; }
        public DbSet<CategoryOfficeTechnique> CategoriesOfficeTechnique { get; set; }
        public DbSet<CategoryTool> CategoriesTool { get; set; }
        
        public DbSet<Log> Logs { get; set; }
        
        
    }
}
