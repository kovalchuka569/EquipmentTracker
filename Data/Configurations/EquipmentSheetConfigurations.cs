using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.EquipmentSheet;

namespace Data.Configurations;

public class EquipmentSheetConfigurations : IEntityTypeConfiguration<EquipmentSheetEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentSheetEntity> builder)
    {
        builder.Property(e => e.ColumnsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");

        builder.Property(e => e.RowsJson)
            .HasColumnType("jsonb")
            .HasDefaultValueSql("'[]'::jsonb");
    }
}