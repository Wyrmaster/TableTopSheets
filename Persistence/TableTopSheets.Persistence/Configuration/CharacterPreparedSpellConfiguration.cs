using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterPreparedSpellConfiguration: IEntityTypeConfiguration<CharacterPreparedSpell>
{
  public void Configure(EntityTypeBuilder<CharacterPreparedSpell> builder)
  {
    builder.HasIndex(entity => new { entity.CharacterId, entity.ClassId, entity.SpellId }).IsUnique();
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(entity => entity.Spell).WithMany().HasForeignKey(entity => entity.SpellId).OnDelete(DeleteBehavior.Restrict);
  }
}
