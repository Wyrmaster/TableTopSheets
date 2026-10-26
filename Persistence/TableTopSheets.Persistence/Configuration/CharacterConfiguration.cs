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
    builder.Property(entity => entity.BackgroundEquipment).HasMaxLength(16);
    builder.Property(entity => entity.ClassEquipment).HasMaxLength(16);
    builder.HasIndex(entity => entity.OwnerId);
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
