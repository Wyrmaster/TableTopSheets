using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class FeatureConfiguration: IEntityTypeConfiguration<Feature>
{
  public void Configure(EntityTypeBuilder<Feature> builder)
  {
    builder.HasIndex(entity => entity.Key).IsUnique();
    builder.HasIndex(entity => entity.Category);
    builder.Property(entity => entity.Key).HasMaxLength(128);
    builder.HasOne(entity => entity.Spell).WithMany().HasForeignKey(entity => entity.SpellId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Effects).WithOne(entity => entity.Feature).HasForeignKey(entity => entity.FeatureId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Choices).WithOne(entity => entity.Feature).HasForeignKey(entity => entity.FeatureId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Resources).WithOne(entity => entity.Feature).HasForeignKey(entity => entity.FeatureId).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(entity => entity.Spellcasting).WithOne(entity => entity.Feature).HasForeignKey<SpellcastingProfile>(entity => entity.FeatureId).OnDelete(DeleteBehavior.Cascade);
  }
}
