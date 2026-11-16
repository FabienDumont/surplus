using System.Globalization;
using Surplus.Application.Settings;
using Surplus.Domain.GameEngine;
using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation;
using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Application.Reporting;

/// <summary>
/// A stretch of days told in a few lines. Every occurrence is still recorded
/// day by day, but a player reads them weighed: what needs them comes one by
/// one and dated, what is merely worth knowing comes summed, and the routine
/// comes as one line. It ends with where the player's own country stands, since
/// that is what the player has to act on.
/// Everything measured is shown in the player's units, which the occurrences'
/// own sentences, written in the world's units, cannot do.
/// </summary>
public sealed record Digest(GameDate From, GameDate To, bool Interrupted, IReadOnlyList<string> Lines)
{
  #region Fields

  /// <summary>The smallest net movement of value over a passage worth a line.</summary>
  private static readonly decimal ReportedMovement = 0.01m;

  /// <summary>
  /// Below this many days of its own basket, a province is named in the digest.
  /// Circulation keeps every province at about ten days, and it eats one before
  /// the next delivery, so a province is only running low once it falls to half
  /// that.
  /// </summary>
  private static readonly decimal ProvincesRunningLow = 5m;

  #endregion

  #region Methods

  public static Digest Of(Passage passage, World world, UnitSystem units)
  {
    if (passage.Reports.Count == 0)
    {
      throw new DomainException("A passage of no days has nothing to tell.");
    }

    var dated = passage.Reports
      .SelectMany(report => report.Occurrences, (report, occurrence) => (report.Date, Occurrence: occurrence))
      .Select(entry => (entry.Date, entry.Occurrence, Importance: Significance.Of(entry.Occurrence, world)))
      .ToList();

    // A trouble that lasts is told once, on the day it began, with how often it came back.
    List<string> lines =
    [
      .. dated.Where(entry => entry.Importance is Importance.Urgent)
        .GroupBy(entry => Significance.Concern(entry.Occurrence))
        .Select(days => Dated(days.First().Date, days.First().Occurrence) +
                        (days.Count() > 1 ? $" The same on {MoreDays(days.Count() - 1)}." : ""))
    ];

    var notable = dated.Where(entry => entry.Importance is Importance.Notable).ToList();

    lines.AddRange(Revaluations(notable.Select(entry => entry.Occurrence).OfType<CommodityWasRevalued>(), world, units));
    lines.AddRange(IdleAbroad(notable.Select(entry => entry.Occurrence).OfType<BranchStoodIdle>()));
    lines.AddRange(FaminesAbroad(notable.Select(entry => entry.Occurrence).OfType<PeopleWentWithout>()));
    lines.AddRange(Worklessness(notable.Select(entry => entry.Occurrence).OfType<TheWorklessWentWithout>()));
    lines.AddRange(Pauperism(notable.Select(entry => entry.Occurrence).OfType<SankIntoPauperism>()));
    lines.AddRange(
      notable.Where(entry => entry.Occurrence is not
          (CommodityWasRevalued or BranchStoodIdle or PeopleWentWithout or TheWorklessWentWithout or SankIntoPauperism))
        .Select(entry => Dated(entry.Date, entry.Occurrence)));
    lines.AddRange(Fronts(dated.Select(entry => entry.Occurrence).OfType<TheFrontMoved>(), world));
    lines.AddRange(Growth(dated.Select(entry => entry.Occurrence).OfType<PopulationGrew>()));

    if (world.Played is { } played)
    {
      lines.Add(Where(played, world));
      lines.AddRange(RunningLow(played, world));
      lines.Add(StoreChange(played, passage.PlayedStoreBefore ?? Store.Empty, world, units));
    }

    return new Digest(passage.Reports[0].Date, passage.Reports[^1].Date, passage.Interrupted, lines);
  }

  private static string Dated(GameDate date, Occurrence occurrence)
  {
    return $"{date}  {occurrence}";
  }

  /// <summary>
  /// One line per commodity, from what it was worth when the passage began to
  /// what it is worth at its end. A value that went up and came back down has
  /// not moved, as far as the player is concerned.
  /// </summary>
  private static IEnumerable<string> Revaluations(
    IEnumerable<CommodityWasRevalued> revaluations, World world, UnitSystem units)
  {
    foreach (var moves in revaluations.GroupBy(revaluation => revaluation.Commodity))
    {
      var before = moves.First().Before;
      var after = moves.Last().After;

      if (Math.Abs(after.RatioTo(before) - 1m) < ReportedMovement)
      {
        continue;
      }

      var unit = world.Register.Commodities.Single(commodity => commodity.Name == moves.Key).UseValue.Unit;

      yield return
        $"{moves.Key}: {Hours(Measures.Shown(before, unit, units))} → {Hours(Measures.Shown(after, unit, units))} h " +
        $"per {Measures.Shown(unit, units)}";
    }
  }

  private static IEnumerable<string> IdleAbroad(IEnumerable<BranchStoodIdle> idle)
  {
    return idle
      .GroupBy(branch => (branch.Country, branch.Province, branch.Commodity))
      .Select(days =>
        $"{days.Key.Country}: {days.Key.Province}'s {days.Key.Commodity} branch stood idle on {Days(days.Count())}.");
  }

  private static IEnumerable<string> Worklessness(IEnumerable<TheWorklessWentWithout> workless)
  {
    return workless
      .GroupBy(day => day.Country)
      .Select(days =>
        $"{days.Key}: those with no wage went short on {Days(days.Count())}; {Number(days.Sum(day => day.Deaths))} died.");
  }

  private static IEnumerable<string> Pauperism(IEnumerable<SankIntoPauperism> sinking)
  {
    return sinking
      .GroupBy(month => month.Country)
      .Select(months => $"{months.Key}: {Number(months.Sum(month => month.Heads))} sank into pauperism.");
  }

  private static IEnumerable<string> FaminesAbroad(IEnumerable<PeopleWentWithout> famines)
  {
    return famines
      .GroupBy(famine => famine.Country)
      .Select(days =>
        $"{days.Key} went short on {Days(days.Count())}; {Number(days.Sum(famine => famine.Deaths))} died.");
  }

  /// <summary>
  /// Where each war still being fought stands at the end of the passage. A war
  /// ended in the meantime is told by its peace, not by its last front.
  /// </summary>
  private static IEnumerable<string> Fronts(IEnumerable<TheFrontMoved> fronts, World world)
  {
    var open = world.Wars.Where(war => !war.IsOver)
      .Select(war => (Aggressor: Named(world, war.Aggressor), Defender: Named(world, war.Defender)))
      .ToHashSet();

    return fronts
      .GroupBy(front => (front.Aggressor, front.Defender))
      .Where(war => open.Contains(war.Key))
      .Select(war => war.Last())
      .Select(front =>
        $"War: {front.Aggressor} on {front.Defender}, {front.Momentum.ToString("+#;-#;0", CultureInfo.InvariantCulture)}, " +
        front.Momentum switch
        {
          > 0 => $"{front.Aggressor} prevailing.",
          < 0 => $"{front.Defender} prevailing.",
          _ => "neither side prevailing."
        });
  }

  private static string Named(World world, CountryId country)
  {
    return world.Countries.Single(candidate => candidate.Id == country).Name;
  }

  private static IEnumerable<string> Growth(IEnumerable<PopulationGrew> growth)
  {
    var byCountry = growth
      .GroupBy(grew => grew.Country)
      .Select(grew => $"{grew.Key} +{Number(grew.Sum(month => month.Heads))}")
      .ToList();

    return byCountry.Count == 0 ? [] : [$"Population grew: {string.Join(", ", byCountry)}."];
  }

  private static string Where(Country country, World world)
  {
    var days = Standing.DaysOfSubsistence(country, world.Subsistence);
    var provisions = days is { } kept
      ? $"{Number(Math.Floor(kept))} days of subsistence in store"
      : "no one to keep";

    return $"{country.Name}: {Number(country.Population)} souls, {provisions}, " +
           $"{Number(Standing.WithoutWork(country))} without work.";
  }

  /// <summary>
  /// The player's provinces whose own store would keep their people fewer
  /// than five days. The country can be well stocked while a province goes
  /// hungry, because what is in Kent is not yet in Lancashire.
  /// </summary>
  private static IEnumerable<string> RunningLow(Country country, World world)
  {
    foreach (var province in country.Provinces)
    {
      if (Standing.DaysOfSubsistence(province, world.Subsistence) is { } days && days < ProvincesRunningLow)
      {
        yield return $"{province.Name}: {Number(Math.Floor(days))} days of subsistence in store.";
      }
    }
  }

  private static string StoreChange(Country country, Store before, World world, UnitSystem units)
  {
    var changes = world.Register.Commodities
      .Select(commodity => (commodity.UseValue.Unit, commodity.Name,
                            Change: country.Store.AmountOf(commodity.Id) - before.AmountOf(commodity.Id)))
      .Where(change => change.Change != 0m)
      .Select(change =>
      {
        var shown = Measures.Shown(Quantity.Of(Math.Abs(change.Change), change.Unit), units);

        return $"{change.Name} {(change.Change > 0m ? "+" : "-")}{Number(shown.Amount)} {shown.Unit}";
      })
      .ToList();

    return changes.Count == 0 ? "Store: unchanged" : $"Store: {string.Join(", ", changes)}";
  }

  private static string MoreDays(int days)
  {
    return days == 1 ? "1 more day" : $"{Number(days)} more days";
  }

  private static string Days(int days)
  {
    return days == 1 ? "1 day" : $"{Number(days)} days";
  }

  private static string Hours(Value value)
  {
    return value.Magnitude.Hours.ToString("0.00", CultureInfo.InvariantCulture);
  }

  private static string Number(decimal number)
  {
    return number.ToString("#,0.##", CultureInfo.InvariantCulture);
  }

  public bool Equals(Digest? other)
  {
    return other is not null
           && From == other.From
           && To == other.To
           && Interrupted == other.Interrupted
           && Lines.SequenceEqual(other.Lines);
  }

  public override int GetHashCode()
  {
    return Lines.Aggregate(HashCode.Combine(From, To, Interrupted), HashCode.Combine);
  }

  #endregion
}
