using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ScaleValueConfiguration: IEntityTypeConfiguration<ScaleValue>
{
  public void Configure(EntityTypeBuilder<ScaleValue> builder)
  {
    builder
      .HasIndex(entity => new { entity.Key, entity.Level })
      .IsUnique();
    builder
      .HasOne(entity => entity.Class)
      .WithMany()
      .HasForeignKey(entity => entity.ClassId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
