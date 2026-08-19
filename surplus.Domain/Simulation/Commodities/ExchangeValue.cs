using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// The exchange-value of a commodity: the proportion in which use-values of one
/// kind exchange for use-values of another kind ("20 yards of linen = 1 coat").
/// It is not an intrinsic property a commodity carries around, but a relation
/// between two commodities — the form of appearance of their common substance,
/// value. It is therefore always derived through <see cref="Between"/> and never
/// stored on <see cref="Commodity"/>.
/// </summary>
public sealed record ExchangeValue
{
    /// <summary>The commodity whose value is being expressed (relative form).</summary>
    public Commodity Relative { get; }

    /// <summary>The commodity in whose body that value is mirrored (equivalent form).</summary>
    public Commodity Equivalent { get; }

    /// <summary>How many units of the equivalent one unit of the relative commodity commands.</summary>
    public decimal Proportion { get; }

    private ExchangeValue(Commodity relative, Commodity equivalent, decimal proportion)
    {
        Relative = relative;
        Equivalent = equivalent;
        Proportion = proportion;
    }

    public static ExchangeValue Between(Commodity relative, Commodity equivalent)
    {
        if (relative.Id == equivalent.Id)
            throw new DomainException("A commodity cannot express its value in its own body.");

        if (relative.UseValue == equivalent.UseValue)
            throw new DomainException(
                "Exchange-value relates use-values of different kinds: " +
                "'x linen = y linen' is no expression of value.");

        return new ExchangeValue(relative, equivalent, relative.Value.RatioTo(equivalent.Value));
    }

    public override string ToString() =>
        $"1 {Relative.UseValue.Unit} of {Relative.Name} = {Proportion} {Equivalent.UseValue.Unit} of {Equivalent.Name}";
}
