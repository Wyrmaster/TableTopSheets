using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterLevelConfiguration: IEntityTypeConfiguration<CharacterLevel>
{
  public void Configure(EntityTypeBuilder<CharacterLevel> builder)
  {
    builder.HasIndex(entity => new { entity.CharacterId, entity.Number }).IsUnique();
    builder.HasOne(entity => entity.Class).WithMany().HasForeignKey(entity => entity.ClassId).OnDelete(DeleteBehavior.Restrict);
    builder.HasOne(entity => entity.Subclass).WithMany().HasForeignKey(entity => entity.SubclassId).OnDelete(DeleteBehavior.Restrict);
  }
}
