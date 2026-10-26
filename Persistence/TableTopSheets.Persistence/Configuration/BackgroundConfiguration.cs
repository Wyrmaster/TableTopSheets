using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class BackgroundConfiguration: RuleEntityConfiguration<Background>
{
  protected override void WithConfigure(EntityTypeBuilder<Background> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Background).HasForeignKey(entity => entity.BackgroundId).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(entity => entity.OriginFeat).WithMany().HasForeignKey(entity => entity.OriginFeatId).OnDelete(DeleteBehavior.SetNull);
    builder.Property(entity => entity.StartingEquipment).HasJsonConversion();
  }
}
