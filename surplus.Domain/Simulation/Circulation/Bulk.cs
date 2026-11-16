namespace Surplus.Domain.Simulation.Circulation;

/// <summary>How many loads of carriage one unit of a commodity takes up.</summary>
public sealed record Bulk(CommodityId Commodity, decimal LoadsPerUnit);
