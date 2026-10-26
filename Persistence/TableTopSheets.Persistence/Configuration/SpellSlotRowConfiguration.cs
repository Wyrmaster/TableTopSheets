using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class SpellSlotRowConfiguration: IEntityTypeConfiguration<SpellSlotRow>
{
  public void Configure(EntityTypeBuilder<SpellSlotRow> builder)
  {
    builder.HasIndex(entity => new { entity.TableKey, entity.CasterLevel, entity.SpellLevel }).IsUnique();
  }
}
