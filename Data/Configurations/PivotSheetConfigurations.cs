using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities;

namespace Data.Configurations;

public class PivotSheetConfigurations : IEntityTypeConfiguration<PivotSheetEntity>
{
    public void Configure(EntityTypeBuilder<PivotSheetEntity> builder)
    {
        builder.ToTable("PivotSheets");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.ColumnsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasMaxLength(int.MaxValue);

        builder.Property(e => e.RowsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb")
            .HasMaxLength(int.MaxValue);
    }
}