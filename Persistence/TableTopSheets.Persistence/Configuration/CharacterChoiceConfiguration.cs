using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

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
