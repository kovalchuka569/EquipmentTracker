using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.SummarySheet;

namespace Data.Configurations.Summary;

public class SummarySheetConfiguration : IEntityTypeConfiguration<SummarySheetEntity>
{
    public void Configure(EntityTypeBuilder<SummarySheetEntity> builder)
    {
        builder
            .HasMany(ss => ss.SummaryColumns)
            .WithOne(ss => ss.SummarySheet)
            .HasForeignKey(ss => ss.SummarySheetId)
            .IsRequired();

        builder
            .HasMany(ss => ss.SummaryRows)
            .WithOne(ss => ss.SummarySheet)
            .HasForeignKey(ss => ss.SummarySheetId)
            .IsRequired();
    }
}