namespace Surplus.Application.Saving.Snapshots;

/// <summary>A heap of one commodity.</summary>
public sealed record StockSnapshot(
  Guid Commodity, decimal Amount, string Unit);
