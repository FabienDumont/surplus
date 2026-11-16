using Surplus.Domain.Simulation.Production;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>A way of working, and what it grows out of.</summary>
public sealed record TechniqueSnapshot(
  string Name, Guid Produces, ProductiveForm Form, decimal OutputPerHour, string OutputUnit, decimal Wear,
  StockSnapshot[] InstrumentsPerHand, StockSnapshot[] MaterialPerHand, string[] Requires, decimal Development);
