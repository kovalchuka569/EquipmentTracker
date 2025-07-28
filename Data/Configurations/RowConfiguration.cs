using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class RowConfiguration : IEntityTypeConfiguration<RowEntity>
{
    public void Configure(EntityTypeBuilder<RowEntity> builder)
    {
        builder
            .HasMany(r => r.Cells)
            .WithOne(c => c.RowEnity)
            .HasForeignKey(c => c.RowId);
    }
}