using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Configuration;

public class RulesetMigrationConfiguration: IEntityTypeConfiguration<RulesetMigration>
{
  public void Configure(EntityTypeBuilder<RulesetMigration> builder)
  {
    builder
      .Property(entity => entity.ContentHash)
      .HasMaxLength(64);
    builder
      .Property(entity => entity.ToolVersion)
      .HasMaxLength(64);
    builder
      .Property(entity => entity.Summary)
      .HasMaxLength(1024);
    builder
      .HasIndex(entity => entity.AppliedAt);
  }
}
