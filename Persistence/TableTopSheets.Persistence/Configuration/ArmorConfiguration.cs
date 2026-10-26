using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ArmorConfiguration: IEntityTypeConfiguration<Armor>
{
  public void Configure(EntityTypeBuilder<Armor> builder)
  {
    builder.Property(entity => entity.Category).HasColumnName("ArmorCategory");
  }
}
