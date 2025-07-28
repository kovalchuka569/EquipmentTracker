using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class ColumnConfiguration : IEntityTypeConfiguration<ColumnEntity>
{
    public void Configure(EntityTypeBuilder<ColumnEntity> builder)
    {
        builder
            .HasMany(c => c.Cells)
            .WithOne(c => c.ColumnEntity)
            .HasForeignKey(c => c.ColumnId);

        builder
            .OwnsOne(c => c.Settings, ownerBuilder => ownerBuilder.ToJson());
    }
}