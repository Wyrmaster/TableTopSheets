namespace TableTopSheets.Common.Sheet;

/// <summary>
///   Run of text with one style inside a block of a rule description
/// </summary>
public sealed record TextSpan(string Text, bool Bold = false, bool Italic = false);
