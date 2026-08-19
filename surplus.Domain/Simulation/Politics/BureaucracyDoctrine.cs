namespace Surplus.Domain.Simulation.Politics;

/// <summary>
/// What a tendency holds the bureaucracy of a workers' state to be. No Marxist
/// tendency has ever agreed with the others on this, and the simulation takes
/// no side: each doctrine is recorded as the position of the ideology that
/// holds it, never as a fact about the world. Which one is right is a question
/// for the player, not for the engine.
/// </summary>
public enum BureaucracyDoctrine
{
  /// <summary>Held by tendencies with no workers' state in view.</summary>
  NotAtIssue,

  /// <summary>The apparatus simply is the dictatorship of the proletariat, and no contradiction arises.</summary>
  OrganOfTheWorkersState,

  /// <summary>A bourgeoisie regenerating inside the party, to be struggled against by the masses themselves.</summary>
  NewBourgeoisie,

  /// <summary>A parasitic caste on a workers' state, removable by political revolution alone.</summary>
  ParasiticCaste,

  /// <summary>Not a workers' state at all: state ownership without workers' power is state capitalism.</summary>
  StateCapitalism
}
