using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Application.Reporting;

/// <summary>
/// A stretch of days let pass in one go: what each day reported, what the
/// player's country held before the first of them, and whether something cut
/// the stretch short.
/// </summary>
public sealed record Passage(IReadOnlyList<DayReport> Reports, Store? PlayedStoreBefore, bool Interrupted)
{
  #region Methods

  public bool Equals(Passage? other)
  {
    return other is not null
           && Reports.SequenceEqual(other.Reports)
           && Equals(PlayedStoreBefore, other.PlayedStoreBefore)
           && Interrupted == other.Interrupted;
  }

  public override int GetHashCode()
  {
    return Reports.Aggregate(HashCode.Combine(PlayedStoreBefore, Interrupted), HashCode.Combine);
  }

  #endregion
}
