namespace Surplus.Application.Saving.Snapshots;

/// <summary>Two provinces that touch.</summary>
public sealed record BorderSnapshot(
  Guid One, Guid Other);
