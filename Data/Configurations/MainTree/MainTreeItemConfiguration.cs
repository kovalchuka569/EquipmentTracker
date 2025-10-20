using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Data.Entities.MainTree;
using Common.Enums;

namespace Data.Configurations.MainTree;

public class MainTreeItemConfiguration : IEntityTypeConfiguration<MainTreeItemEntity>
{
    public void Configure(EntityTypeBuilder<MainTreeItemEntity> builder)
    {
        builder.ToTable("MainTreeItems");
        
        builder.HasKey(f => f.Id);

        builder.HasDiscriminator(f => f.Format)
            .HasValue<MainTreeItemEntity>(FileFormat.None)
            .HasValue<FolderEntity>(FileFormat.Folder)
            .HasValue<EquipmentFileEntity>(FileFormat.EquipmentSheet)
            .HasValue<PivotFileEntity>(FileFormat.PivotSheet);

        builder.Property(f => f.Name)
            .HasMaxLength(200);
    }
}