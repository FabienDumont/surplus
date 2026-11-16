using System.Globalization;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// People with no title of their own went short, however full the store. It is
/// not a famine: the goods were there. It is the condition of the reserve army,
/// and of the paupers below it.
/// </summary>
public sealed record TheWorklessWentWithout(string Country, int Heads, int Deaths) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    var workless = Heads.ToString("#,0", CultureInfo.InvariantCulture);

    return Deaths == 0
      ? $"In {Country} {workless} had no wage to live on; no one died of it today."
      : $"In {Country} {workless} had no wage to live on; {Deaths.ToString("#,0", CultureInfo.InvariantCulture)} died.";
  }

  #endregion
}
