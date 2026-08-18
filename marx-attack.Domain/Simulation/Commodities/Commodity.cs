using MarxAttack.Domain.Simulation.Production;
using MarxAttack.Domain.SharedKernel;

namespace MarxAttack.Domain.Simulation.Commodities;

/// <summary>
/// Aggregate root: a useful thing produced by labour for exchange.
/// Its two factors are its use-value and its value.
/// Invariants:
///  - a commodity is always a use-value: nothing can have value without being
///    an object of utility (enforced by the non-nullable <see cref="UseValue"/>);
///  - a commodity is always a product of labour: a useful thing owing nothing
///    to labour (air, virgin soil) is a use-value but no commodity;
///  - exchange-value is deliberately absent from this state: it is a relation
///    between commodities (<see cref="ExchangeValue"/>), the mere form of
///    appearance of value, never a property carried by one commodity alone.
/// </summary>
public sealed class Commodity
{
    public CommodityId Id { get; }
    public string Name { get; }
    public UseValue UseValue { get; }
    public Value Value { get; }

    private Commodity(CommodityId id, string name, UseValue useValue, Value value)
    {
        Id = id;
        Name = name;
        UseValue = useValue;
        Value = value;
    }

    /// <summary>
    /// Brings a commodity into the world. Its value is determined by the labour
    /// time <em>socially necessary</em> for its production, not by the time any
    /// individual producer happened to spend.
    /// </summary>
    public static Commodity Produce(string name, UseValue useValue, LaborTime sociallyNecessaryLaborTime)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("A commodity must have a name.");

        if (sociallyNecessaryLaborTime.IsNone)
            throw new DomainException(
                "A useful thing that owes nothing to labour is a use-value, not a commodity.");

        return new Commodity(
            CommodityId.New(),
            name.Trim(),
            useValue,
            Value.CrystallisedFrom(sociallyNecessaryLaborTime));
    }

    public override string ToString() => $"{Name} ({UseValue}, {Value})";
}
