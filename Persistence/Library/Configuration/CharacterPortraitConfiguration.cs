using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterPortraitConfiguration: IEntityTypeConfiguration<CharacterPortrait>
{
  public void Configure(EntityTypeBuilder<CharacterPortrait> builder)
  {
    builder
      .Property(entity => entity.ContentType)
      .HasMaxLength(64);
    builder
      .HasIndex(entity => entity.CharacterId)
      .IsUnique();
  }
}
