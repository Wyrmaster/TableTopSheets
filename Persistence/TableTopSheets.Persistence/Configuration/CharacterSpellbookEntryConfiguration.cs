using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterSpellbookEntryConfiguration: IEntityTypeConfiguration<CharacterSpellbookEntry>
{
  public void Configure(EntityTypeBuilder<CharacterSpellbookEntry> builder)
  {
    builder
      .HasOne(entity => entity.Spell)
      .WithMany()
      .HasForeignKey(entity => entity.SpellId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
