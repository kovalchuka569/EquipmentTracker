using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.FileSystem;

namespace Data.Configurations;

public class SummaryFileConfiguration : IEntityTypeConfiguration<SummaryFileEntity>
{
    public void Configure(EntityTypeBuilder<SummaryFileEntity> builder)
    {
        builder
            .HasOne(sfe => sfe.SummarySheet)
            .WithOne()
            .HasForeignKey<SummaryFileEntity>(sfe => sfe.SummarySheetId)
            .IsRequired();
    }
}