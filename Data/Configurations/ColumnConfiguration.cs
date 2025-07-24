using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<ColumnEntity>
{
    public void Configure(EntityTypeBuilder<ColumnEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder
            .HasMany(c => c.Cells)
            .WithOne(c => c.ColumnEntity)
            .HasForeignKey(c => c.ColumnId);
        
        builder
            .HasOne(c => c.TableEntity)
            .WithMany(t => t.Columns)
            .HasForeignKey(c => c.TableId);
    }
}