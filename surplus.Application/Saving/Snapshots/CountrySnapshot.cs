using Surplus.Domain.Simulation.Countries;

namespace Surplus.Application.Saving.Snapshots;

/// <summary>A country: who plays it, its state, its provinces and what it knows.</summary>
public sealed record CountrySnapshot(
  Guid Id, string Name, CountryControl Control, GovernmentSnapshot Government, ProvinceSnapshot[] Provinces,
  string[] Known, DevelopmentSnapshot[] UnderWay);
