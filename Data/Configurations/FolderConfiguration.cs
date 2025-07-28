using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.FileSystem;
using Models.Enums;

namespace Data.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<FolderEntity>
{
    public void Configure(EntityTypeBuilder<FolderEntity> builder)
    {
        
        builder.HasKey(c => c.Id);
        
        builder.HasMany(f => f.Folders)
            .WithOne(f => f.Folder)
            .HasForeignKey(f => f.FolderId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(f => f.Files)
            .WithOne(f => f.Folder)
            .HasForeignKey(f => f.FolderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        var menuTypeConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<MenuType, string>(
            v => v.ToString(),
            v => (MenuType)Enum.Parse(typeof(MenuType), v)
        );
        
        builder
            .Property(f => f.MenuType)
            .HasConversion(menuTypeConverter);
    }
}