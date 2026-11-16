namespace Surplus.Application.Saving.Snapshots;

/// <summary>The root of a save file: its version, the clock and the world.</summary>
public sealed record SessionSnapshot(
  int Version, GameSnapshot Game, WorldSnapshot World);
