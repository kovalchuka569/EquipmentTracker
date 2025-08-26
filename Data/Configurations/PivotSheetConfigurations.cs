using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Models.Entities.PivotSheet;

namespace Data.Configurations;

public class PivotSheetConfigurations : IEntityTypeConfiguration<PivotSheetEntity>
{
    public void Configure(EntityTypeBuilder<PivotSheetEntity> builder)
    {
        builder.Property(e => e.ColumnsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(e => e.RowsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");
    }
}