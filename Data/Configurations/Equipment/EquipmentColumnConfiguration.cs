using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.EquipmentSheet;

namespace Data.Configurations.Equipment;

public class EquipmentColumnConfiguration : IEntityTypeConfiguration<EquipmentColumnEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentColumnEntity> builder)
    {
        builder.HasKey(ec => new { ec.EquipmentSheetId, ec.ColumnId });

        builder
            .HasOne(ec => ec.EquipmentSheet)
            .WithMany(ec => ec.EquipmentColumns)
            .HasForeignKey(ec => ec.EquipmentSheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ec => ec.ColumnEntity)
            .WithMany()
            .HasForeignKey(ec => ec.ColumnId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}