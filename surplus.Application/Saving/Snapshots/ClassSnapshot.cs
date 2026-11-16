using Surplus.Domain.Simulation.Society;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>So many heads of one class.</summary>
public sealed record ClassSnapshot(
  SocialClass Class, int Heads);
