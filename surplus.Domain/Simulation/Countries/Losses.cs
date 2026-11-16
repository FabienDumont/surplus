namespace Surplus.Domain.Simulation.Countries;

/// <summary>
/// The day's dead of want, by cause: those whose title the store could not
/// honour, and those who had no title to begin with.
/// </summary>
public sealed record Losses(int OfShortage, int OfWorklessness);
