namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>
/// A country has found out a new way of working: enough labour has gone into
/// the trades it grows out of. Finding it out is not installing it; that still
/// takes the instruments, and a class that can work them.
/// </summary>
public sealed record TechniqueWasDeveloped(string Country, string Technique, string Commodity) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Country} has found out how to work by {Technique}, a new way of making {Commodity}.";
  }

  #endregion
}
