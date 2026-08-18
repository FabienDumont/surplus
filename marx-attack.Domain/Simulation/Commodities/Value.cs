using MarxAttack.Domain.Simulation.Production;

namespace MarxAttack.Domain.Simulation.Commodities;

/// <summary>
/// The value crystallised in a commodity.
/// Substance of value: abstract human labour, labour stripped of its concrete,
/// useful character — enforced here by the single factory, which accepts nothing
/// but labour time.
/// Magnitude of value: the labour time socially necessary for production.
/// All values share the same homogeneous substance, so any two values are
/// commensurable — unlike use-values.
/// </summary>
public sealed record Value : IComparable<Value>
{
    /// <summary>
    /// Things can be use-values without being values: air, virgin soil,
    /// natural meadows owe their utility to no labour.
    /// </summary>
    public static readonly Value None = new(LaborTime.None);

    public LaborTime Magnitude { get; }

    private Value(LaborTime magnitude) => Magnitude = magnitude;

    /// <summary>Value has no source other than labour.</summary>
    public static Value CrystallisedFrom(LaborTime sociallyNecessaryLaborTime) =>
        new(sociallyNecessaryLaborTime);

    public bool IsNone => Magnitude.IsNone;

    /// <summary>
    /// The definite quantitative proportion between two values — what surfaces
    /// as exchange-value when two commodities face each other.
    /// </summary>
    public decimal RatioTo(Value other) => Magnitude.RatioTo(other.Magnitude);

    public static Value operator +(Value left, Value right) =>
        new(left.Magnitude + right.Magnitude);

    public int CompareTo(Value? other) =>
        other is null ? 1 : Magnitude.CompareTo(other.Magnitude);

    public override string ToString() => $"value of {Magnitude}";
}
