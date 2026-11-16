using Surplus.Domain.Simulation.Warfare;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>The terms a war ended on, and the provinces ceded under them.</summary>
public sealed record PeaceSnapshot(
  PeaceTerms Terms, Guid[] Ceded);
