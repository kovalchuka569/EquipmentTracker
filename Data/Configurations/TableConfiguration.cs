using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class TableConfiguration : IEntityTypeConfiguration<TableEntity>
{
    public void Configure(EntityTypeBuilder<TableEntity> builder)
    {
        builder.HasKey(t => t.Id);

        builder
            .HasMany(t => t.Rows)
            .WithOne(r => r.TableEntity)
            .HasForeignKey(r => r.TableId);
        
        builder
            .HasMany(t => t.Columns)
            .WithOne(c => c.TableEntity)
            .HasForeignKey(c => c.TableId);
    }
}