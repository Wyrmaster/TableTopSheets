namespace TableTopSheets.Seed;

/// <summary>
///   What <see cref="SeedWriter.WriteAsync"/> changed in the database
/// </summary>
/// <param name="Inserted">rows that didn't exist before</param>
/// <param name="Updated">existing rows that were written again</param>
/// <param name="Deleted">stale child rows of seeded rules that the seed no longer contains</param>
public sealed record SeedWriteReport(int Inserted, int Updated, int Deleted);
