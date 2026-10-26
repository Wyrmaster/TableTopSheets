using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class SpeciesConfiguration: RuleEntityConfiguration<Species>
{
  protected override void WithConfigure(EntityTypeBuilder<Species> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Species).HasForeignKey(entity => entity.SpeciesId).OnDelete(DeleteBehavior.Cascade);
  }
}
