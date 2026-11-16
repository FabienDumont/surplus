using Surplus.Domain.GameEngine;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// What one elapsed day came to, in the order it happened. Most days nothing
/// worth telling happens: the same labour is done, the same products consumed,
/// and reproduction on the same scale leaves no mark — which is itself the
/// report.
/// </summary>
public sealed record DayReport(GameDate Date, IReadOnlyList<Occurrence> Occurrences)
{
  #region Methods

  /// <summary>The occurrences of one kind, in the order they happened.</summary>
  public IEnumerable<T> Of<T>() where T : Occurrence
  {
    return Occurrences.OfType<T>();
  }

  public bool Equals(DayReport? other)
  {
    return other is not null
           && Date == other.Date
           && Occurrences.SequenceEqual(other.Occurrences);
  }

  public override int GetHashCode()
  {
    return Occurrences.Aggregate(Date.GetHashCode(), HashCode.Combine);
  }

  #endregion
}
