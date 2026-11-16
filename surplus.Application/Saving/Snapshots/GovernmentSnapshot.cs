using Surplus.Domain.Simulation.Politics;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>How a state is constituted, what it avows, its statutes and whether it relieves the poor.</summary>
public sealed record GovernmentSnapshot(
  GovernmentForm Form, Ideology Ideology, Law[] Laws, bool RelievesThePoor);
