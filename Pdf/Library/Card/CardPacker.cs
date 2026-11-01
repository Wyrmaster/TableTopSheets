using PdfSharp.Drawing;

namespace TableTopSheets.Pdf.Card;

/// <summary>
///   Packs cards of all sizes onto as few A4 portrait pages as possible: MaxRects bin packing with the best short side
///   fit, 90° rotation allowed. Every card keeps <see cref="Gap"/> to its neighbours and stays inside
///   <see cref="Margin"/>.
/// </summary>
public static class CardPacker
{
  #region Constants

  /// <summary>
  ///   Width of an A4 page
  /// </summary>
  public static readonly double PageWidth = XUnit.FromMillimeter(210).Point;

  /// <summary>
  ///   Height of an A4 page
  /// </summary>
  public static readonly double PageHeight = XUnit.FromMillimeter(297).Point;

  /// <summary>
  ///   Printable margin around the cards
  /// </summary>
  public static readonly double Margin = XUnit.FromMillimeter(5).Point;

  /// <summary>
  ///   Distance between two cards, room for the crop marks
  /// </summary>
  public static readonly double Gap = XUnit.FromMillimeter(3).Point;

  private const double Epsilon = 0.01;

  #endregion

  #region Public Methods

  /// <summary>
  ///   Pages of placed cards. The cards go in by area, largest first (in input order within a size), each to the best
  ///   free spot of any page; a new page starts only when a card fits nowhere. A card tries the orientation that fits
  ///   most cards of its size onto a page first and turns only to fill a hole the other one doesn't fit.
  /// </summary>
  public static IReadOnlyList<IReadOnlyList<CardPlacement>> Pack(IReadOnlyList<CardPart> parts)
  {
    // every card and the area are enlarged by the gap: cards that touch there are a gap apart on the page
    double binWidth = PageWidth - 2 * Margin + Gap;
    double binHeight = PageHeight - 2 * Margin + Gap;

    List<Bin> bins = [];
    IEnumerable<CardPart> ordered = parts.OrderByDescending(part => part.Format.Width * part.Format.Height);
    foreach (CardPart part in ordered)
    {
      double width = part.Format.Width + Gap;
      double height = part.Format.Height + Gap;
      bool preferRotated = PerPage(binWidth, binHeight, height, width) > PerPage(binWidth, binHeight, width, height);
      (double Width, double Height, bool Rotated)[] orientations = preferRotated
        ? [(height, width, true), (width, height, false)]
        : [(width, height, false), (height, width, true)];

      bool placed = false;
      foreach ((double w, double h, bool rotated) in orientations)
      {
        (Bin Bin, XRect Rect, double Short, double Long)? best = null;
        foreach (Bin bin in bins)
        {
          if (bin.Find(w, h) is { } spot && (best == null || (spot.Short, spot.Long).CompareTo((best.Value.Short, best.Value.Long)) < 0))
          {
            best = (bin, spot.Rect, spot.Short, spot.Long);
          }
        }

        if (best is { } found)
        {
          found.Bin.Place(part, found.Rect, rotated);
          placed = true;
          break;
        }
      }

      if (placed)
      {
        continue;
      }

      Bin page = new(binWidth, binHeight);
      bins.Add(page);
      (double Width, double Height, bool Rotated)? fits = orientations
        .Cast<(double Width, double Height, bool Rotated)?>()
        .FirstOrDefault(entry => page.Find(entry!.Value.Width, entry.Value.Height) != null);
      if (fits is not { } fit)
      {
        throw new ArgumentException($"A card of {part.Format.Size} is larger than the page", nameof(parts));
      }

      page.Place(part, page.Find(fit.Width, fit.Height)!.Value.Rect, fit.Rotated);
    }

    return bins
      .Select(bin => (IReadOnlyList<CardPlacement>)bin.Placements
        .Select(placement => placement with
        {
          Bounds = new(placement.Bounds.X + Margin, placement.Bounds.Y + Margin, placement.Bounds.Width - Gap, placement.Bounds.Height - Gap),
        })
        .ToList())
      .ToList();
  }

  #endregion

  #region Private Methods

  /// <summary>
  ///   Cards of one orientation in a plain grid on an empty page
  /// </summary>
  private static int PerPage(double binWidth, double binHeight, double width, double height) =>
    (int)Math.Floor((binWidth + Epsilon) / width) * (int)Math.Floor((binHeight + Epsilon) / height);

  #endregion

  #region Nested Types

  /// <summary>
  ///   One page: the placed cards and the maximal free rectangles between them
  /// </summary>
  private sealed class Bin
  {
    #region Field

    private readonly List<XRect> _free;

    #endregion

    #region Constructor

    public Bin(double width, double height)
    {
      this._free = [new XRect(0, 0, width, height)];
    }

    #endregion

    #region Property

    /// <summary>
    ///   Placed cards with their bounds enlarged by the gap, relative to the margin
    /// </summary>
    public List<CardPlacement> Placements { get; } = [];

    #endregion

    #region Public Methods

    /// <summary>
    ///   Best short side fit of a <paramref name="width"/> × <paramref name="height"/> rectangle; ties go to the best long side
    /// </summary>
    public (XRect Rect, double Short, double Long)? Find(double width, double height)
    {
      (XRect Rect, double Short, double Long)? best = null;
      foreach (XRect free in this._free)
      {
        if (width > free.Width + Epsilon || height > free.Height + Epsilon)
        {
          continue;
        }

        double leftX = free.Width - width;
        double leftY = free.Height - height;
        double shortSide = Math.Min(leftX, leftY);
        double longSide = Math.Max(leftX, leftY);
        if (best == null || (shortSide, longSide).CompareTo((best.Value.Short, best.Value.Long)) < 0)
        {
          best = (new XRect(free.X, free.Y, width, height), shortSide, longSide);
        }
      }

      return best;
    }

    /// <summary>
    ///   Place a card and split every free rectangle it overlaps into the parts around it
    /// </summary>
    public void Place(CardPart part, XRect rect, bool rotated)
    {
      this.Placements.Add(new(part, rect, rotated));

      List<XRect> next = [];
      foreach (XRect free in this._free)
      {
        if (!Overlaps(free, rect))
        {
          next.Add(free);
          continue;
        }

        if (rect.X > free.X + Epsilon)
        {
          next.Add(new(free.X, free.Y, rect.X - free.X, free.Height));
        }

        if (rect.Right < free.Right - Epsilon)
        {
          next.Add(new(rect.Right, free.Y, free.Right - rect.Right, free.Height));
        }

        if (rect.Y > free.Y + Epsilon)
        {
          next.Add(new(free.X, free.Y, free.Width, rect.Y - free.Y));
        }

        if (rect.Bottom < free.Bottom - Epsilon)
        {
          next.Add(new(free.X, rect.Bottom, free.Width, free.Bottom - rect.Bottom));
        }
      }

      // keep only maximal rectangles
      this._free.Clear();
      for (int index = 0; index < next.Count; index++)
      {
        XRect candidate = next[index];
        bool contained = next
          .Where((other, otherIndex) => otherIndex != index && Contains(other, candidate) && (!Contains(candidate, other) || otherIndex < index))
          .Any();
        if (!contained)
        {
          this._free.Add(candidate);
        }
      }
    }

    #endregion

    #region Private Methods

    private static bool Overlaps(XRect a, XRect b) =>
      a.X < b.Right - Epsilon && b.X < a.Right - Epsilon && a.Y < b.Bottom - Epsilon && b.Y < a.Bottom - Epsilon;

    private static bool Contains(XRect outer, XRect inner) =>
      inner.X >= outer.X - Epsilon && inner.Y >= outer.Y - Epsilon && inner.Right <= outer.Right + Epsilon && inner.Bottom <= outer.Bottom + Epsilon;

    #endregion
  }

  #endregion
}
