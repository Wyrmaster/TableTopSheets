using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ChoiceConfiguration: IEntityTypeConfiguration<Choice>
{
  public void Configure(EntityTypeBuilder<Choice> builder)
  {
    builder.HasIndex(entity => entity.Key).IsUnique();
    builder.Property(entity => entity.Key).HasMaxLength(128);
    builder.HasMany(entity => entity.Options).WithOne(entity => entity.Choice).HasForeignKey(entity => entity.ChoiceId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Effects).WithOne(entity => entity.Choice).HasForeignKey(entity => entity.ChoiceId).OnDelete(DeleteBehavior.Cascade);
  }
}
