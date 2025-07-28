using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.EquipmentSheet;

namespace Data.Configurations.Equipment;

public class EquipmentSheetConfigurations : IEntityTypeConfiguration<EquipmentSheetEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentSheetEntity> builder)
    {
        builder
            .HasMany(e => e.EquipmentRows)
            .WithOne(e => e.EquipmentSheet)
            .HasForeignKey(e => e.EquipmentSheetId)
            .IsRequired();
        
        builder
            .HasMany(e => e.EquipmentColumns)
            .WithOne(e => e.EquipmentSheet)
            .HasForeignKey(e => e.EquipmentSheetId)
            .IsRequired();
    }
}