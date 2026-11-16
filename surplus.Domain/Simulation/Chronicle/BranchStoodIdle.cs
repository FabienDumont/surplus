namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A branch that did no work today because its means of production were not in
/// the store. Production is reproduction or it is nothing: a branch that the
/// rest of the social product does not replace stops, however many hands it
/// has standing ready.
/// </summary>
public sealed record BranchStoodIdle(string Country, string Province, string Commodity) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Province}'s {Commodity} branch stood idle for want of material ({Country}).";
  }

  #endregion
}
