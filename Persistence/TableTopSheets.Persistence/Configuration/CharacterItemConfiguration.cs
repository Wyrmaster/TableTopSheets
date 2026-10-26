using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Characters;

namespace TableTopSheets.Persistence.Configuration;

public class CharacterItemConfiguration: IEntityTypeConfiguration<CharacterItem>
{
  public void Configure(EntityTypeBuilder<CharacterItem> builder)
  {
    builder.Ignore(entity => entity.Equipped);
    builder.HasOne(entity => entity.Item).WithMany().HasForeignKey(entity => entity.ItemId).OnDelete(DeleteBehavior.Restrict);
  }
}
