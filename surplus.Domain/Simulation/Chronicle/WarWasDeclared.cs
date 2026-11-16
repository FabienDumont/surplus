namespace Surplus.Domain.Simulation.Chronicle;

/// <summary>One state has gone to war with another.</summary>
public sealed record WarWasDeclared(string Aggressor, string Defender) : Occurrence
{
  #region Methods

  public override string ToString()
  {
    return $"{Aggressor} has declared war on {Defender}.";
  }

  #endregion
}
