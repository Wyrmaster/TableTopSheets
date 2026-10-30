namespace TableTopSheets.Seed.Images;

/// <summary>
///   What <see cref="ImageWriter.WriteAsync"/> changed in the database
/// </summary>
/// <param name="Inserted">images that didn't exist before</param>
/// <param name="Updated">images whose content changed</param>
/// <param name="Deleted">images whose file is gone</param>
/// <param name="Unchanged">images that were already up to date</param>
public sealed record ImageWriteReport(int Inserted, int Updated, int Deleted, int Unchanged);
