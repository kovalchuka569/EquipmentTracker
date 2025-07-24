using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class RowConfiguration : IEntityTypeConfiguration<RowEntity>
{
    public void Configure(EntityTypeBuilder<RowEntity> builder)
    {
        builder.HasKey(r => r.Id);

        builder
            .HasMany(r => r.Cells)
            .WithOne(c => c.RowEnity)
            .HasForeignKey(c => c.RowId);

        builder
            .HasOne(r => r.TableEntity)
            .WithMany(t => t.Rows)
            .HasForeignKey(r => r.TableId);
    }
}