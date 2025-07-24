using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.Table;

namespace Data.Configurations;

public class CellConfiguration : IEntityTypeConfiguration<CellEntity>
{
    public void Configure(EntityTypeBuilder<CellEntity> builder)
    {
        builder.HasKey(c => c.Id);

        builder
            .HasOne(c => c.ColumnEntity)
            .WithMany(c => c.Cells)
            .HasForeignKey(c => c.ColumnId);
        
        builder
            .HasOne(c => c.RowEnity)
            .WithMany(r => r.Cells)
            .HasForeignKey(c => c.RowId);
    }
}