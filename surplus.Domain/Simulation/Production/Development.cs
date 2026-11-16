namespace Surplus.Domain.Simulation.Production;

/// <summary>A technique partly found, and the weighed labour spent toward it so far.</summary>
public sealed record Development(string Technique, LaborTime Spent);
