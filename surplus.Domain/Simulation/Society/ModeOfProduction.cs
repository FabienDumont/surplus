namespace Surplus.Domain.Simulation.Society;

/// <summary>
/// The economic structure of a society — "the specific economic form in which
/// unpaid surplus-labour is pumped out of the direct producers", which Marx
/// calls the hidden basis of the entire social structure (Capital III, ch. 47).
/// This is the base, never the superstructure: how a country is governed is a
/// separate question from how its surplus is extracted. That is why fascism
/// appears nowhere in this enum — it is a political form of capitalist rule,
/// not a mode of production of its own, and belongs on the country's regime.
/// </summary>
public enum ModeOfProduction
{
  /// <summary>No classes: the conditions of production are held in common by the gens.</summary>
  PrimitiveCommunal,

  /// <summary>Surplus pumped out by owning the producer outright.</summary>
  Slave,

  /// <summary>Surplus pumped out by extra-economic coercion: corvée, rent in kind, money rent.</summary>
  Feudal,

  /// <summary>Surplus pumped out through the wage, which makes all labour appear paid.</summary>
  Capitalist,

  /// <summary>The lower phase: the producers hold power, but classes and the state survive.</summary>
  Socialist,

  /// <summary>The higher phase: classes abolished, and with them the state.</summary>
  Communist
}
