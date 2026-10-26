using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterClassConfiguration: RuleEntityConfiguration<CharacterClass>
{
  protected override void WithConfigure(EntityTypeBuilder<CharacterClass> builder)
  {
    builder
      .HasMany(entity => entity.Subclasses)
      .WithOne(entity => entity.Class)
      .HasForeignKey(entity => entity.ClassId)
      .OnDelete(DeleteBehavior.Cascade);
    builder
      .HasMany(entity => entity.Features)
      .WithOne(entity => entity.Class)
      .HasForeignKey(entity => entity.ClassId)
      .OnDelete(DeleteBehavior.Cascade);
    builder
      .Property(entity => entity.StartingEquipment)
      .HasJsonConversion();
  }
}
