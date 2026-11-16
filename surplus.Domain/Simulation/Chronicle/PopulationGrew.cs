using System.Globalization;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>A country whose people, fed through the month, multiplied.</summary>
public sealed record PopulationGrew(string Country, int Heads) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Country} grew by {Heads.ToString("#,0", CultureInfo.InvariantCulture)} {(Heads == 1 ? "soul" : "souls")}.";
  }

  #endregion
}
