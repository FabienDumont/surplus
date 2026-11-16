using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Application.Reporting;

/// <summary>Where a country stands toward one technique of the catalogue, as a player reads it.</summary>
public static class Mastery
{
  #region Methods

  /// <summary>
  /// One of: known; under way, with the share found so far rounded down; out
  /// of reach until its missing prerequisites are known; or, for an elementary
  /// technique, simply not known.
  /// </summary>
  public static string Of(Country country, Technique technique)
  {
    if (country.Knows(technique))
    {
      return "known";
    }

    if (technique.IsElementary)
    {
      return "not known";
    }

    var missing = technique.Requires.Where(name => !country.Knowledge.Knows(name)).ToList();

    if (missing.Count > 0)
    {
      return $"out of reach until {string.Join(" and ", missing)} {(missing.Count == 1 ? "is" : "are")} known";
    }

    var share = Math.Floor(country.Knowledge.Spent(technique.Name).RatioTo(technique.Development) * 100m);

    return $"under way, {share:0}%";
  }

  #endregion
}
