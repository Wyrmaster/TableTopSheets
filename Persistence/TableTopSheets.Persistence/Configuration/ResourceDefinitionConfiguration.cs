using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class ResourceDefinitionConfiguration: IEntityTypeConfiguration<ResourceDefinition>
{
  public void Configure(EntityTypeBuilder<ResourceDefinition> builder)
  {
    builder.HasIndex(entity => entity.Key).IsUnique();
  }
}
