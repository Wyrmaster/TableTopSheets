using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterResourceUsageConfiguration: IEntityTypeConfiguration<CharacterResourceUsage>
{
  public void Configure(EntityTypeBuilder<CharacterResourceUsage> builder)
  {
    builder
      .HasIndex(entity => new { entity.CharacterId, entity.Key })
      .IsUnique();
  }
}
