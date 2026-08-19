using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Simulation.Commodities;

/// <summary>
/// Aggregate root: a useful thing produced by labour for exchange.
/// Its two factors are its use-value and its value.
/// Invariants:
/// - a commodity is always a use-value: nothing can have value without being
/// an object of utility (enforced by the non-nullable <see cref="UseValue" />);
/// - a commodity is always a product of labour: a useful thing owing nothing
/// to labour (air, virgin soil) is a use-value but no commodity;
/// - exchange-value is deliberately absent from this state: it is a relation
/// between commodities (<see cref="ExchangeValue" />), the mere form of
/// appearance of value, never a property carried by one commodity alone.
/// This is the commodity as a kind, not a heap of it: what a definite mass of it
/// amounts to is <see cref="Stock" />. Value belongs here rather than there
/// because it is social — one and the same for every yard of linen in the
/// market, whoever wove it.
/// </summary>
public sealed class Commodity
{
  #region Properties

  public CommodityId Id { get; }
  public string Name { get; }
  public UseValue UseValue { get; }

  /// <summary>Which consumption this commodity enters, and so which department it belongs to.</summary>
  public Department Department { get; }

  /// <summary>
  /// The value of one unit of this commodity. Not fixed at birth: it is the
  /// labour society needs to reproduce the thing now, and that changes
  /// (<see cref="Revalue" />).
  /// </summary>
  public Value Value { get; private set; }

  #endregion

  #region Ctors

  private Commodity(CommodityId id, string name, UseValue useValue, Department department, Value value)
  {
    Id = id;
    Name = name;
    UseValue = useValue;
    Department = department;
    Value = value;
  }

  #endregion

  #region Methods

  /// <summary>
  /// Brings a commodity into the world. Its value is determined by the labour
  /// time <em>socially necessary</em> for its production, not by the time any
  /// individual producer happened to spend.
  /// </summary>
  public static Commodity Produce(
    string name, UseValue useValue, Department department, LaborTime sociallyNecessaryLaborTime)
  {
    if (string.IsNullOrWhiteSpace(name))
    {
      throw new DomainException("A commodity must have a name.");
    }

    if (sociallyNecessaryLaborTime.IsNone)
    {
      throw new DomainException("A useful thing that owes nothing to labour is a use-value, not a commodity.");
    }

    return new Commodity(
      CommodityId.New(), name.Trim(), useValue, department, Value.CrystallisedFrom(sociallyNecessaryLaborTime)
    );
  }

  /// <summary>
  /// Reconstitutes a commodity from a stored snapshot. Unlike
  /// <see cref="Produce" /> this asserts no invariant: the state it receives
  /// was already valid when it was saved.
  /// </summary>
  public static Commodity Load(
    CommodityId id, string name, UseValue useValue, Department department, Value value)
  {
    return new Commodity(id, name, useValue, department, value);
  }

  /// <summary>
  /// Sets the value to what reproduction now costs. Value is never the labour a
  /// thing once took, always the labour society needs to make it again: let the
  /// power-loom halve the labour of weaving and every yard of linen in
  /// existence falls with it, the hand-woven along with the rest. Nothing is
  /// destroyed and no one is robbed, yet the weaver is poorer — which is the
  /// whole cruelty of the thing, and the reason value is revised here rather
  /// than fixed at birth.
  /// </summary>
  public void Revalue(LaborTime sociallyNecessaryLaborTime)
  {
    if (sociallyNecessaryLaborTime.IsNone)
    {
      throw new DomainException($"{Name} whose reproduction costs no labour has ceased to be a commodity.");
    }

    Value = Value.CrystallisedFrom(sociallyNecessaryLaborTime);
  }

  public override string ToString()
  {
    return $"{Name} ({UseValue}, {Value})";
  }

  #endregion
}
