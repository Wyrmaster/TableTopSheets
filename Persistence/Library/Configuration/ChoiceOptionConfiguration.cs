using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ChoiceOptionConfiguration: IEntityTypeConfiguration<ChoiceOption>
{
  public void Configure(EntityTypeBuilder<ChoiceOption> builder)
  {
    builder
      .HasIndex(entity => new { entity.ChoiceId, entity.Key })
      .IsUnique();
    builder
      .HasOne(entity => entity.GrantsFeature)
      .WithMany()
      .HasForeignKey(entity => entity.GrantsFeatureId)
      .OnDelete(DeleteBehavior.SetNull);
  }
}
