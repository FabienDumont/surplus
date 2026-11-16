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
    var needs = subsistence.For(country.Population).Where(need => !need.IsExhausted).ToList();

    return needs.Count == 0
      ? null
      : needs.Min(need => country.Store.AmountOf(need.Commodity) / need.Quantity.Amount);
  }

  /// <summary>
  /// The direct producers no branch employs. Those who live on the labour of
  /// others are never counted: being without work is not their condition.
  /// </summary>
  public static int WithoutWork(Country country)
  {
    return country.Provinces.Sum(
      province => province.Composition.Presences
        .Where(presence => presence.Profile.IsDirectProducer)
        .Sum(presence => province.Idle(presence.Class)));
  }

  #endregion
}
