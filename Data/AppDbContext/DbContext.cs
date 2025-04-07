using Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Syncfusion.PMML;
using TableColumn = Data.Entities.TableColumn;

namespace Data.AppDbContext
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbContext(DbContextOptions<DbContext> options) : base(options)
        {
            Console.WriteLine("AppDbContext Created!");
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }
        
        ~DbContext()
        {
            Console.WriteLine("AppDbContext Finalized!");
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
