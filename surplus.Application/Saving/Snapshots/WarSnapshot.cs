namespace Surplus.Application.Saving.Snapshots;

/// <summary>A war, open or concluded, with the peace that ended it.</summary>
public sealed record WarSnapshot(
  Guid Id, Guid Aggressor, Guid Defender, int Momentum, PeaceSnapshot? Peace);
