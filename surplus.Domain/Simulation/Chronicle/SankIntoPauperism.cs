using System.Globalization;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// Workers out of work long enough to fall out of the working class altogether,
/// into the lumpenproletariat, which no branch will employ again.
/// </summary>
public sealed record SankIntoPauperism(string Country, int Heads) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"In {Country} {Heads.ToString("#,0", CultureInfo.InvariantCulture)} sank into pauperism.";
  }

  #endregion
}
