using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Persistence.Configuration;

public class RuleImageConfiguration: IEntityTypeConfiguration<RuleImage>
{
  public void Configure(EntityTypeBuilder<RuleImage> builder)
  {
    builder.ToTable("Images");
    builder
      .Property(entity => entity.OwnerKey)
      .HasMaxLength(128);
    builder
      .Property(entity => entity.Title)
      .HasMaxLength(256);
    builder
      .Property(entity => entity.ContentType)
      .HasMaxLength(64);
    builder
      .Property(entity => entity.ContentHash)
      .HasMaxLength(64);
    builder
      .HasIndex(entity => new { entity.Category, entity.OwnerKey })
      .IsUnique();
  }
}
