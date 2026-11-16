namespace Surplus.Application.Saving.Snapshots;

/// <summary>A technique partly found, and the weighed hours spent toward it.</summary>
public sealed record DevelopmentSnapshot(
  string Technique, decimal Spent);
