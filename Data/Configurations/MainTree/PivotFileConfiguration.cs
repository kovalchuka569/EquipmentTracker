using Data.Entities.MainTree;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Data.Configurations.MainTree;

public class PivotFileConfiguration : IEntityTypeConfiguration<PivotFileEntity>
{
    public void Configure(EntityTypeBuilder<PivotFileEntity> builder)
    {
        builder.HasOne(e => e.PivotSheet)
            .WithMany()
            .HasForeignKey(e => e.PivotSheetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}