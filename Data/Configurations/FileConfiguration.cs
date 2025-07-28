using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Models.Entities.FileSystem;
using Models.Enums;

namespace Data.Configurations;

public class FileConfiguration : IEntityTypeConfiguration<FileEntity>
{
    public void Configure(EntityTypeBuilder<FileEntity> builder)
    {
        
        builder.HasKey(c => c.Id);

        builder.HasDiscriminator<string>("Discriminator")
            .HasValue<FileEntity>("BaseFile")
            .HasValue<EquipmentFileEntity>("EquipmentSheet")
            .HasValue<SummaryFileEntity>("SummarySheet");

        builder
            .HasOne(f => f.Folder)
            .WithMany(f => f.Files);
        
        var menuTypeConverter = new ValueConverter<MenuType, string>(
            v => v.ToString(),
            v => (MenuType)Enum.Parse(typeof(MenuType), v)
            );

        var fileFormatConverter = new ValueConverter<FileFormat, string>(
            v => v.ToString(),
            v => (FileFormat)Enum.Parse(typeof(FileFormat), v)
            ); 

        builder
            .Property(f => f.MenuType)
            .HasConversion(menuTypeConverter);
        
        builder
            .Property(f => f.FileFormat)
            .HasConversion(fileFormatConverter);
        
    }
}