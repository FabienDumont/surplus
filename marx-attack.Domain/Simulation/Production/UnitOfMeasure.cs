using MarxAttack.Domain.SharedKernel;

namespace MarxAttack.Domain.Simulation.Production;

/// <summary>
/// The natural unit in which a use-value is counted or measured
/// (yards of linen, single coats, quarters of wheat).
/// Each use-value carries its own unit; units of different use-values
/// are not interchangeable.
/// </summary>
public sealed record UnitOfMeasure
{
    public string Name { get; }

    private UnitOfMeasure(string name) => Name = name;

    public static UnitOfMeasure Of(string name) =>
        string.IsNullOrWhiteSpace(name)
            ? throw new DomainException("A unit of measure must have a name.")
            : new UnitOfMeasure(name.Trim());

    public override string ToString() => Name;
}
