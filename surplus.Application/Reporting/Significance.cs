using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Application.Reporting;

/// <summary>
/// Weighs what happened for the one who reads it. The world records every
/// occurrence alike; whether one stops the clock depends on whose country it
/// befell, which is a question about the player and not about the world. That
/// is why this lives in the application layer.
/// </summary>
public static class Significance
{
  #region Methods

  /// <summary>
  /// How much an occurrence matters to the player. Anything not classified here
  /// counts as notable, so that an occurrence added later is shown until someone
  /// decides it deserves less.
  /// </summary>
  public static Importance Of(Occurrence occurrence, World world)
  {
    return occurrence switch
    {
      PeopleWentWithout famine => AtHomeOrAbroad(famine.Country, world),
      BranchStoodIdle idle => AtHomeOrAbroad(idle.Country, world),
      CommodityWasRevalued => Importance.Notable,
      PopulationGrew => Importance.Routine,
      // What the player did needs no stopping for; what was done to them does.
      WarWasDeclared declared => AtHomeOrAbroad(declared.Defender, world),
      PeaceWasMade peace => AtHomeOrAbroad(peace.Loser, world),
      TheFrontMoved => Importance.Routine,
      _ => Importance.Notable
    };
  }

  /// <summary>
  /// Whether a day brought something urgent that the day before did not: the
  /// rule by which the clock stops for the player. A trouble already under way
  /// stopped the clock when it began. Stopping again every day it lasts would
  /// only make the player wade through it a day at a time.
  /// </summary>
  public static bool CallsForAttention(DayReport report, DayReport? dayBefore, World world)
  {
    var already = dayBefore?.Occurrences.Select(Concern).ToHashSet() ?? [];

    return report.Occurrences.Any(
      occurrence => Of(occurrence, world) is Importance.Urgent && !already.Contains(Concern(occurrence)));
  }

  /// <summary>
  /// What an occurrence is about, so that the same trouble on two days is
  /// recognised as one. A famine is the same famine whatever share was
  /// provided; an idle branch is the same branch. Anything else is only ever
  /// the same as itself.
  /// </summary>
  public static object Concern(Occurrence occurrence)
  {
    return occurrence switch
    {
      PeopleWentWithout famine => (nameof(PeopleWentWithout), famine.Country),
      BranchStoodIdle idle => (nameof(BranchStoodIdle), idle.Country, idle.Province, idle.Commodity),
      _ => occurrence
    };
  }

  private static Importance AtHomeOrAbroad(string country, World world)
  {
    return world.Played?.Name == country ? Importance.Urgent : Importance.Notable;
  }

  #endregion
}
