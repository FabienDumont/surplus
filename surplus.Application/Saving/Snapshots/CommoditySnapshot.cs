using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>A commodity in play and its value today, in hours a unit.</summary>
public sealed record CommoditySnapshot(
  Guid Id, string Name, string SatisfiedWant, string Unit, Department Department, decimal Value);
