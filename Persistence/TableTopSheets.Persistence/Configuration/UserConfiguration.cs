using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Persistence.Entity.Users;

namespace TableTopSheets.Persistence.Configuration;

public class UserConfiguration: IEntityTypeConfiguration<User>
{
  public void Configure(EntityTypeBuilder<User> builder)
  {
    builder.Property(entity => entity.UserName).HasMaxLength(64);
    builder.Property(entity => entity.Email).HasMaxLength(256);
    builder.Property(entity => entity.PasswordHash).HasMaxLength(512);
    builder.Property(entity => entity.ExternalIssuer).HasMaxLength(512);
    builder.Property(entity => entity.ExternalSubject).HasMaxLength(256);
    builder.HasIndex(entity => entity.UserName).IsUnique();
    builder.HasIndex(entity => new { entity.ExternalIssuer, entity.ExternalSubject }).IsUnique();
    builder.HasMany(entity => entity.Characters).WithOne(entity => entity.Owner).HasForeignKey(entity => entity.OwnerId).OnDelete(DeleteBehavior.Cascade);
  }
}
