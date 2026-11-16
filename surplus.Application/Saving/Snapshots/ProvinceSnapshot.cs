namespace Surplus.Application.Saving.Snapshots;

/// <summary>A province: its classes in order, its branches in order, and its own store.</summary>
public sealed record ProvinceSnapshot(
  Guid Id, string Name, ClassSnapshot[] Classes, BranchSnapshot[] Branches, StockSnapshot[] Store);
