namespace Surplus.Domain.Simulation.Warfare;

/// <summary>
/// What a war can be brought to an end on. A victor takes what the fighting has
/// actually put within reach and no more: most wars settle far short of
/// swallowing the loser whole, and many settle for nothing at all.
/// </summary>
public enum PeaceTerms
{
  /// <summary>The guns stop and nothing changes hands. Always available to either side.</summary>
  White,

  /// <summary>Named provinces change hands, as many as the fighting has earned.</summary>
  Cession,

  /// <summary>The defeated state ceases to exist and its whole territory passes to the victor.</summary>
  Annexation
}
