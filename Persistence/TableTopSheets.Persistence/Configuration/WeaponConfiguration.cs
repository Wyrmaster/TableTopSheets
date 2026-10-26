using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

public class WeaponConfiguration: IEntityTypeConfiguration<Weapon>
{
  public void Configure(EntityTypeBuilder<Weapon> builder)
  {
    builder.HasOne(entity => entity.Mastery).WithMany().HasForeignKey(entity => entity.MasteryId).OnDelete(DeleteBehavior.SetNull);
  }
}
