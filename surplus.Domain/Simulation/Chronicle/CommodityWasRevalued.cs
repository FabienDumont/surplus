using System.Globalization;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A commodity whose value moved appreciably today. Nobody decided it: the
/// labour society needed to make the thing changed, and with it what every unit
/// of it in every store is worth.
/// </summary>
public sealed record CommodityWasRevalued(string Commodity, string Unit, Value Before, Value After) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Commodity} now holds {Hours(After)}h of labour per {Unit}, " +
           $"{(After.CompareTo(Before) < 0 ? "down" : "up")} from {Hours(Before)}h.";
  }

  private static string Hours(Value value)
  {
    return value.Magnitude.Hours.ToString("0.00", CultureInfo.InvariantCulture);
  }

  #endregion
}
