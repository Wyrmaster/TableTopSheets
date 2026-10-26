using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ItemConfiguration: RuleEntityConfiguration<Item>
{
  protected override void WithConfigure(EntityTypeBuilder<Item> builder)
  {
    builder
      .HasDiscriminator(entity => entity.Kind)
      .HasValue<Item>(ItemKindEnum.Gear)
      .HasValue<Weapon>(ItemKindEnum.Weapon)
      .HasValue<Armor>(ItemKindEnum.Armor)
      .HasValue<Tool>(ItemKindEnum.Tool);
    builder.Property(entity => entity.Group).HasMaxLength(64);
    builder.Property(entity => entity.Bundle).HasDefaultValue(1);
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Item).HasForeignKey(entity => entity.ItemId).OnDelete(DeleteBehavior.Cascade);
  }
}
