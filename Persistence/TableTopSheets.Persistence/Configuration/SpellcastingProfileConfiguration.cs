using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class SpellcastingProfileConfiguration: IEntityTypeConfiguration<SpellcastingProfile>
{
  public void Configure(EntityTypeBuilder<SpellcastingProfile> builder)
  {
    builder
      .HasOne(entity => entity.Class)
      .WithMany()
      .HasForeignKey(entity => entity.ClassId)
      .OnDelete(DeleteBehavior.Restrict);
  }
}
