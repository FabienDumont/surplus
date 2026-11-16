namespace Surplus.Application.Saving.Snapshots;

/// <summary>What carrying costs; no carriage at all when <c>Carriage</c> is null.</summary>
public sealed record HaulageSnapshot(
  Guid? Carriage, BulkSnapshot[] Bulk);
