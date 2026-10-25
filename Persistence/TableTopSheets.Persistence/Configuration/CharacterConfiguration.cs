using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterConfiguration: IEntityTypeConfiguration<Character>
{
  public void Configure(EntityTypeBuilder<Character> builder)
  {
    builder.Property(entity => entity.Name).HasMaxLength(256);
    builder.OwnsOne(entity => entity.PlayState);
    builder.HasOne(entity => entity.Species).WithMany().HasForeignKey(entity => entity.SpeciesId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(entity => entity.Background).WithMany().HasForeignKey(entity => entity.BackgroundId).OnDelete(DeleteBehavior.Restrict);
    builder.HasMany(entity => entity.Levels).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Choices).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.Items).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.PreparedSpells).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.SpellbookEntries).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
    builder.HasMany(entity => entity.ResourceUsages).WithOne(entity => entity.Character).HasForeignKey(entity => entity.CharacterId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class CharacterLevelConfiguration: IEntityTypeConfiguration<CharacterLevel>
{
  public void Configure(EntityTypeBuilder<CharacterLevel> builder)
  {
    builder.HasIndex(entity => new { entity.CharacterId, entity.Number }).IsUnique();
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(entity => entity.Subclass).WithMany().HasForeignKey(entity => entity.SubclassId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class CharacterChoiceConfiguration: IEntityTypeConfiguration<CharacterChoice>
{
  public void Configure(EntityTypeBuilder<CharacterChoice> builder)
  {
    builder.Property(entity => entity.OptionKey).HasMaxLength(128);
    builder.HasIndex(entity => new { entity.CharacterId, entity.ChoiceId });
    builder.HasOne(entity => entity.Choice).WithMany().HasForeignKey(entity => entity.ChoiceId).OnDelete(DeleteBehavior.Restrict);

    // undoing a level up removes the level row: selections made with it are deleted, selections it replaced become active again
    builder.HasOne(entity => entity.Level).WithMany().HasForeignKey(entity => entity.LevelId).OnDelete(DeleteBehavior.Cascade);
    builder.HasOne(entity => entity.SupersededByLevel).WithMany().HasForeignKey(entity => entity.SupersededByLevelId).OnDelete(DeleteBehavior.SetNull);
    builder.HasOne(entity => entity.Parent).WithMany(entity => entity.Children).HasForeignKey(entity => entity.ParentId).OnDelete(DeleteBehavior.Cascade);
  }
}

public class CharacterItemConfiguration: IEntityTypeConfiguration<CharacterItem>
{
  public void Configure(EntityTypeBuilder<CharacterItem> builder)
  {
    builder.Ignore(entity => entity.Equipped);
    builder.HasOne(entity => entity.Item).WithMany().HasForeignKey(entity => entity.ItemId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class CharacterPreparedSpellConfiguration: IEntityTypeConfiguration<CharacterPreparedSpell>
{
  public void Configure(EntityTypeBuilder<CharacterPreparedSpell> builder)
  {
    builder.HasIndex(entity => new { entity.CharacterId, entity.ClassId, entity.SpellId }).IsUnique();
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(entity => entity.Spell).WithMany().HasForeignKey(entity => entity.SpellId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class CharacterSpellbookEntryConfiguration: IEntityTypeConfiguration<CharacterSpellbookEntry>
{
  public void Configure(EntityTypeBuilder<CharacterSpellbookEntry> builder)
  {
    builder.HasOne(entity => entity.Spell).WithMany().HasForeignKey(entity => entity.SpellId).OnDelete(DeleteBehavior.Restrict);
  }
}

public class CharacterResourceUsageConfiguration: IEntityTypeConfiguration<CharacterResourceUsage>
{
  public void Configure(EntityTypeBuilder<CharacterResourceUsage> builder)
  {
    builder.HasIndex(entity => new { entity.CharacterId, entity.Key }).IsUnique();
  }
}
