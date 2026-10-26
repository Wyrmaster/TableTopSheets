using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class SubclassConfiguration: RuleEntityConfiguration<Subclass>
{
  protected override void WithConfigure(EntityTypeBuilder<Subclass> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Subclass).HasForeignKey(entity => entity.SubclassId).OnDelete(DeleteBehavior.Cascade);
  }
}
