using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.EquipmentSheet;

namespace Data.Configurations.Equipment;

public class EquipmentRowConfiguration : IEntityTypeConfiguration<EquipmentRowEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentRowEntity> builder)
    {
        builder.HasKey(er => new {er.EquipmentSheetId, er.RowId });
        
        builder
            .HasOne(er => er.EquipmentSheet)
            .WithMany(er => er.EquipmentRows)
            .HasForeignKey(er => er.RowId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(er => er.Row)
            .WithMany()
            .HasForeignKey(er => er.RowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}