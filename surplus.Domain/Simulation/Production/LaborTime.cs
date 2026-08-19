using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A quantity of undifferentiated, homogeneous human labour.
/// Socially necessary labour time is the measure of the magnitude of value.
/// </summary>
public readonly record struct LaborTime : IComparable<LaborTime>
{
    public static readonly LaborTime None = new(0m);

    public decimal Hours { get; }

    private LaborTime(decimal hours) => Hours = hours;

    public static LaborTime FromHours(decimal hours) =>
        hours < 0m
            ? throw new DomainException("Labour time cannot be negative.")
            : new LaborTime(hours);

    public bool IsNone => Hours == 0m;

    public static LaborTime operator +(LaborTime left, LaborTime right) =>
        new(left.Hours + right.Hours);

    /// <summary>
    /// Labour times are homogeneous, so any two of them stand in a definite
    /// quantitative proportion to one another.
    /// </summary>
    public decimal RatioTo(LaborTime other) =>
        other.IsNone
            ? throw new DomainException("No proportion can be formed with a thing that contains no labour.")
            : Hours / other.Hours;

    public int CompareTo(LaborTime other) => Hours.CompareTo(other.Hours);

    public override string ToString() => $"{Hours}h of labour";
}
