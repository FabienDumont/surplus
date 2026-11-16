namespace Surplus.Application.Saving.Snapshots;

/// <summary>Everything the simulation holds, in the order that decides outcomes.</summary>
public sealed record WorldSnapshot(
  CommoditySnapshot[] Commodities, TechniqueSnapshot[] Techniques, StockSnapshot[] Subsistence, CountrySnapshot[] Countries,
  BorderSnapshot[] Borders, HaulageSnapshot Haulage, WarSnapshot[] Wars, OccurrenceSnapshot[] Pending);
