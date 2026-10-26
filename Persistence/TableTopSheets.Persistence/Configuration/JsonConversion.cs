using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TableTopSheets.Persistence.Configuration;

/// <summary>
///   Stores a value as a JSON text column (the same on every provider)
/// </summary>
public static class JsonConversion
{
  #region Fields

  private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

  #endregion

  #region Methods

  /// <summary>
  ///   Serialize the property to JSON; values are compared by their JSON, so changes inside lists are detected
  /// </summary>
  public static PropertyBuilder<T> HasJsonConversion<T>(this PropertyBuilder<T> builder) where T : new()
  {
    ValueComparer<T> comparer = new(
      (left, right) => Serialize(left) == Serialize(right),
      value => Serialize(value).GetHashCode(StringComparison.Ordinal),
      value => Deserialize<T>(Serialize(value)));
    return builder
      .HasConversion(value => Serialize(value), json => Deserialize<T>(json), comparer)
      .IsRequired();
  }

  private static string Serialize<T>(T? value) => JsonSerializer.Serialize(value, Options);

  private static T Deserialize<T>(string json) where T : new() => JsonSerializer.Deserialize<T>(json, Options) ?? new T();

  #endregion
}
