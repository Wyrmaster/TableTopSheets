using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

/// <summary>
///   Shared configuration of all <see cref="RuleEntity"/> types
/// </summary>
public abstract class RuleEntityConfiguration<TEntity>: IEntityTypeConfiguration<TEntity> where TEntity : RuleEntity
{
  #region Methods

  public void Configure(EntityTypeBuilder<TEntity> builder)
  {
    builder.HasKey(entity => entity.Id);
    builder.Property(entity => entity.Key).HasMaxLength(128);
    builder.Property(entity => entity.Name).HasMaxLength(256);
    builder.HasIndex(entity => entity.Key).IsUnique();
    builder.HasOne(entity => entity.Source).WithMany().HasForeignKey(entity => entity.SourceId).OnDelete(DeleteBehavior.SetNull);

    this.WithConfigure(builder);
  }

  protected virtual void WithConfigure(EntityTypeBuilder<TEntity> builder)
  {
  }

  #endregion
}

public class SourceConfiguration: IEntityTypeConfiguration<Source>
{
  public void Configure(EntityTypeBuilder<Source> builder)
  {
    builder.HasIndex(entity => entity.Key).IsUnique();
  }
}

public class CharacterClassConfiguration: RuleEntityConfiguration<CharacterClass>
{
  protected override void WithConfigure(EntityTypeBuilder<CharacterClass> builder)
  {
    builder.HasMany(entity => entity.Subclasses).WithOne(entity => entity.Class).HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Class).HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Cascade);
    builder.Property(entity => entity.StartingEquipment).HasJsonConversion();
  }
}

public class SubclassConfiguration: RuleEntityConfiguration<Subclass>
{
  protected override void WithConfigure(EntityTypeBuilder<Subclass> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Subclass).HasForeignKey(entity => entity.SubclassId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class SpeciesConfiguration: RuleEntityConfiguration<Species>
{
  protected override void WithConfigure(EntityTypeBuilder<Species> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Species).HasForeignKey(entity => entity.SpeciesId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class BackgroundConfiguration: RuleEntityConfiguration<Background>
{
  protected override void WithConfigure(EntityTypeBuilder<Background> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Background).HasForeignKey(entity => entity.BackgroundId).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(entity => entity.OriginFeat).WithMany().HasForeignKey(entity => entity.OriginFeatId).OnDelete(DeleteBehavior.SetNull);
    builder.Property(entity => entity.StartingEquipment).HasJsonConversion();
  }
}

public class FeatConfiguration: RuleEntityConfiguration<Feat>
{
  protected override void WithConfigure(EntityTypeBuilder<Feat> builder)
  {
    builder.HasMany(entity => entity.Features).WithOne(entity => entity.Feat).HasForeignKey(entity => entity.FeatId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class SpellConfiguration: RuleEntityConfiguration<Spell>;

public class LanguageConfiguration: RuleEntityConfiguration<Language>;

public class WeaponMasteryConfiguration: RuleEntityConfiguration<WeaponMastery>;

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

public class WeaponConfiguration: IEntityTypeConfiguration<Weapon>
{
  public void Configure(EntityTypeBuilder<Weapon> builder)
  {
    builder.HasOne(entity => entity.Mastery).WithMany().HasForeignKey(entity => entity.MasteryId).OnDelete(DeleteBehavior.SetNull);
  }
}

public class ArmorConfiguration: IEntityTypeConfiguration<Armor>
{
  public void Configure(EntityTypeBuilder<Armor> builder)
  {
    builder.Property(entity => entity.Category).HasColumnName("ArmorCategory");
  }
}

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

public class ChoiceOptionConfiguration: IEntityTypeConfiguration<ChoiceOption>
{
  public void Configure(EntityTypeBuilder<ChoiceOption> builder)
  {
    builder.HasIndex(entity => new { entity.ChoiceId, entity.Key }).IsUnique();
    builder.HasOne(entity => entity.GrantsFeature).WithMany().HasForeignKey(entity => entity.GrantsFeatureId).OnDelete(DeleteBehavior.SetNull);
  }
}

public class ScaleValueConfiguration: IEntityTypeConfiguration<ScaleValue>
{
  public void Configure(EntityTypeBuilder<ScaleValue> builder)
  {
    builder.HasIndex(entity => new { entity.Key, entity.Level }).IsUnique();
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class ResourceDefinitionConfiguration: IEntityTypeConfiguration<ResourceDefinition>
{
  public void Configure(EntityTypeBuilder<ResourceDefinition> builder)
  {
    builder.HasIndex(entity => entity.Key).IsUnique();
  }
}

public class SpellcastingProfileConfiguration: IEntityTypeConfiguration<SpellcastingProfile>
{
  public void Configure(EntityTypeBuilder<SpellcastingProfile> builder)
  {
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class SpellSlotRowConfiguration: IEntityTypeConfiguration<SpellSlotRow>
{
  public void Configure(EntityTypeBuilder<SpellSlotRow> builder)
  {
    builder.HasIndex(entity => new { entity.TableKey, entity.CasterLevel, entity.SpellLevel }).IsUnique();
  }
}
