using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Data.Entities;

namespace Data.Configurations;

public class EquipmentSheetConfigurations : IEntityTypeConfiguration<EquipmentSheetEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentSheetEntity> builder)
    {
        builder.ToTable("EquipmentSheets");
        
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