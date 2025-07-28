using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.SummarySheet;

namespace Data.Configurations.Summary;

public class SummaryRowConfiguration : IEntityTypeConfiguration<SummaryRowEntity>
{
    public void Configure(EntityTypeBuilder<SummaryRowEntity> builder)
    {
        builder.HasKey(c => new{c.SummarySheetId, c.RowId});

        builder
            .HasOne(s => s.SummarySheet)
            .WithMany(s => s.SummaryRows)
            .HasForeignKey(s => s.SummarySheetId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder
            .HasOne(s => s.RowEntity)
            .WithMany()
            .HasForeignKey(f => f.RowId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}