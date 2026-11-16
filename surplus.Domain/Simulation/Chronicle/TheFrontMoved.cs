using System.Globalization;

namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// Where a war stands after a month's fighting: positive while the aggressor
/// prevails, negative while the defender does. Told every month a war is open,
/// whether the front moved or not, since a front that holds is news too.
/// </summary>
public sealed record TheFrontMoved(string Aggressor, string Defender, int Momentum) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Aggressor}'s war on {Defender} stands at {Momentum.ToString("+#;-#;0", CultureInfo.InvariantCulture)}.";
  }

  #endregion
}
