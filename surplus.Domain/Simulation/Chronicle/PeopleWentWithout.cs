using System.Globalization;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A country whose store could not give everyone their basket today. The share
/// is rounded down, so that a people a hair short of its subsistence is never
/// told it had all of it.
/// </summary>
public sealed record PeopleWentWithout(string Country, decimal Provided, int Deaths) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    var share = Math.Floor(Provided * 100m).ToString("0", CultureInfo.InvariantCulture);

    return Deaths == 0
      ? $"{Country} could provide only {share}% of its people's subsistence; no one died of it today."
      : $"{Country} could provide only {share}% of its people's subsistence; " +
        $"{Deaths.ToString("#,0", CultureInfo.InvariantCulture)} died.";
  }

  #endregion
}
