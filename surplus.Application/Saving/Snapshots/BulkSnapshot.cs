namespace Surplus.Application.Saving.Snapshots;

/// <summary>The loads one unit of a commodity takes up.</summary>
public sealed record BulkSnapshot(
  Guid Commodity, decimal LoadsPerUnit);
