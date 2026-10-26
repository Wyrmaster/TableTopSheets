using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TableTopSheets.Common.Enum.Rules;
using TableTopSheets.Persistence.Entity.Rules;
using TableTopSheets.Persistence.Entity.Rules.Items;

namespace TableTopSheets.Persistence.Configuration;

/// <summary>
///   Shared configuration of all <see cref="RuleEntity"/> types
/// </summary>
public abstract class RuleEntityConfiguration<TEntity>: IEntityTypeConfiguration<TEntity> where TEntity : RuleEntity
{
  #region Methods

  public void Configure(EntityTypeBuilder<TEntity> builder)
  {
    builder.HasKey(entity => entity.Id);
    builder.Property(entity => entity.Key).HasMaxLength(128);
    builder.Property(entity => entity.Name).HasMaxLength(256);
    builder.HasIndex(entity => entity.Key).IsUnique();
    builder.HasOne(entity => entity.Source).WithMany().HasForeignKey(entity => entity.SourceId).OnDelete(DeleteBehavior.SetNull);

    this.WithConfigure(builder);
  }

  protected virtual void WithConfigure(EntityTypeBuilder<TEntity> builder)
  {
  }

  #endregion
}
