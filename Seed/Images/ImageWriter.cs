using Microsoft.EntityFrameworkCore;
using TableTopSheets.Persistence;
using TableTopSheets.Persistence.Entity.Rules;

namespace TableTopSheets.Seed.Images;

/// <summary>
///   Brings the image table in line with the seed's image files. Images belong to the seed alone: new ones are inserted,
///   changed ones (other content hash) written again and images whose file is gone removed. Unchanged images are not
///   touched, so a run without changes reads only ids and hashes
/// </summary>
public static class ImageWriter
{
  #region Methods

  public static async Task<ImageWriteReport> WriteAsync(TableTopSheetsContext context, IReadOnlyList<RuleImage> images, CancellationToken cancellationToken = default)
  {
    Dictionary<Guid, string> existing = await context.Images
      .AsNoTracking()
      .ToDictionaryAsync(entity => entity.Id, entity => entity.ContentHash, cancellationToken);

    int inserted = 0;
    int updated = 0;
    foreach (RuleImage image in images)
    {
      if (!existing.TryGetValue(image.Id, out string? hash))
      {
        context.Images.Add(image);
        inserted++;
      }
      else if (hash != image.ContentHash)
      {
        context.Images.Update(image);
        updated++;
      }
    }

    HashSet<Guid> current = images
      .Select(image => image.Id)
      .ToHashSet();
    List<Guid> stale = existing.Keys
      .Where(id => !current.Contains(id))
      .ToList();

    await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
    await context.SaveChangesAsync(cancellationToken);
    if (stale.Count > 0)
    {
      await context.Images
        .Where(entity => stale.Contains(entity.Id))
        .ExecuteDeleteAsync(cancellationToken);
    }

    await transaction.CommitAsync(cancellationToken);
    context.ChangeTracker.Clear();

    return new ImageWriteReport(inserted, updated, stale.Count, images.Count - inserted - updated);
  }

  #endregion
}
