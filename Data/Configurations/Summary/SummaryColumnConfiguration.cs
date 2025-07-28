using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Models.Entities.SummarySheet;
using Models.Entities.Table;

namespace Data.Configurations.Summary;

public class SummaryColumnConfiguration : IEntityTypeConfiguration<SummaryColumnEntity>
{
    public void Configure(EntityTypeBuilder<SummaryColumnEntity> builder)
    {
        builder.HasKey(c => new{c.SummarySheetId, c.ColumnId});
        
        builder
            .HasOne(ec => ec.SummarySheet)
            .WithMany(ec => ec.SummaryColumns)
            .HasForeignKey(ec => ec.SummarySheetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(ec => ec.ColumnEntity)
            .WithMany()
            .HasForeignKey(ec => ec.ColumnId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder
            .HasOne<ColumnEntity>()
            .WithMany()
            .HasForeignKey(sc => sc.MergedWith)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
            
    }
}