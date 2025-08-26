using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Models.Entities.FileSystem;

using Common.Enums;

namespace Data.Configurations;

public class FileSystemItemConfiguration : IEntityTypeConfiguration<FileSystemItemEntity>
{
    public void Configure(EntityTypeBuilder<FileSystemItemEntity> builder)
    {
        builder.HasKey(f => f.Id);

        builder.HasDiscriminator(f => f.Format)
            .HasValue<FileSystemItemEntity>(FileFormat.None)
            .HasValue<FolderEntity>(FileFormat.Folder)
            .HasValue<EquipmentFileEntity>(FileFormat.EquipmentSheet)
            .HasValue<PivotFileEntity>(FileFormat.PivotSheet);
        
    }
}