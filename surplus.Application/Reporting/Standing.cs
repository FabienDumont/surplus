using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Countries;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Application.Reporting;

/// <summary>Readings of where a country stands, shared by the digest and the console.</summary>
public static class Standing
{
  #region Methods

  /// <summary>
  /// How many days the store would keep everyone, judged by the scarcest good
  /// in the basket, since the basket is taken whole or in proportion. Null for a
  /// country with no one to keep.
  /// </summary>
  public static decimal? DaysOfSubsistence(Country country, Subsistence subsistence)
  {
    return DaysOf(country.Store, country.Population, subsistence);
  }

  /// <summary>The same reading for one province, out of its own store.</summary>
  public static decimal? DaysOfSubsistence(Province province, Subsistence subsistence)
  {
    return DaysOf(province.Store, province.Population, subsistence);
  }

  private static decimal? DaysOf(Store store, int heads, Subsistence subsistence)
  {
    var needs = subsistence.For(heads).Where(need => !need.IsExhausted).ToList();

    return needs.Count == 0
      ? null
      : needs.Min(need => store.AmountOf(need.Commodity) / need.Quantity.Amount);
  }

  /// <summary>
  /// The reserve army: wage-earners no branch employs. A peasant or a craftsman
  /// outside every branch still lives off the plot or the workshop, and those
  /// who live on others' labour are never without work in this sense, so only
  /// the classes that live by the wage are counted.
  /// </summary>
  public static int WithoutWork(Country country)
  {
    return country.Provinces.Sum(
      province => province.Composition.Presences
        .Where(LivesByTheWage)
        .Sum(presence => province.Idle(presence.Class)));
  }

  /// <summary>Whether a class has nothing to live on but a wage.</summary>
  public static bool LivesByTheWage(ClassPresence presence)
  {
    return presence.Profile.Income.Title() is Access.Employed;
  }

  #endregion
}
