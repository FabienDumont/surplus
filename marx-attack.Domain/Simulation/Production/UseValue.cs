using MarxAttack.Domain.SharedKernel;

namespace MarxAttack.Domain.Simulation.Production;

/// <summary>
/// The qualitative factor of a commodity: the utility of its physical body,
/// the human want it satisfies.
/// Use-values of different kinds differ in quality, not in quantity, and are
/// therefore incommensurable: this type intentionally exposes no ordering and
/// no arithmetic. Only value, whose substance is homogeneous abstract labour,
/// is comparable.
/// </summary>
public sealed record UseValue
{
    /// <summary>The human want this use-value satisfies (warmth, nourishment, clothing…).</summary>
    public string SatisfiedWant { get; }

    /// <summary>The unit in which this particular usefulness is measured.</summary>
    public UnitOfMeasure Unit { get; }

    private UseValue(string satisfiedWant, UnitOfMeasure unit)
    {
        SatisfiedWant = satisfiedWant;
        Unit = unit;
    }

    public static UseValue Of(string satisfiedWant, UnitOfMeasure unit) =>
        string.IsNullOrWhiteSpace(satisfiedWant)
            ? throw new DomainException("A use-value must satisfy some human want.")
            : new UseValue(satisfiedWant.Trim(), unit);

    public override string ToString() => $"satisfies the want for {SatisfiedWant}";
}
