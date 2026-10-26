using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class FeatConfiguration: RuleEntityConfiguration<Feat>
{
  protected override void WithConfigure(EntityTypeBuilder<Feat> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Feat).HasForeignKey(entity => entity.FeatId).OnDelete(DeleteBehavior.Cascade);
  }
}
