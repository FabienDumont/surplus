using Surplus.Domain.Simulation.Production;
using Surplus.Domain.Simulation.Society;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>A branch as it stands today: its means, its hands, its day and its productivity.</summary>
public sealed record BranchSnapshot(
  Guid Produces, ProductiveForm Form, decimal Wear, StockSnapshot[] Instruments, StockSnapshot[] Material,
  SocialClass Workforce, int Hands, decimal WorkingDay, decimal NecessaryLabor, decimal OutputPerHour, string OutputUnit);
