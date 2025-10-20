using Data.Entities.MainTree;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.MainTree;

public class EquipmentFileConfiguration : IEntityTypeConfiguration<EquipmentFileEntity>
{
    public void Configure(EntityTypeBuilder<EquipmentFileEntity> builder)
    {
        builder.HasOne(e => e.EquipmentSheet)
            .WithMany()
            .HasForeignKey(e => e.EquipmentSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}