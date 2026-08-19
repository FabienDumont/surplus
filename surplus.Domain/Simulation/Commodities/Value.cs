using Surplus.Domain.Simulation.Production;

namespace Surplus.Domain.Simulation.Commodities;

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
  #region Fields

  /// <summary>
  /// Things can be use-values without being values: air, virgin soil,
  /// natural meadows owe their utility to no labour.
  /// </summary>
  public static readonly Value None = new(LaborTime.None);

  #endregion

  #region Properties

  public LaborTime Magnitude { get; }

  public bool IsNone => Magnitude.IsNone;

  #endregion

  #region Ctors

  private Value(LaborTime magnitude)
  {
    Magnitude = magnitude;
  }

  #endregion

  #region Methods

  /// <summary>Value has no source other than labour.</summary>
  public static Value CrystallisedFrom(LaborTime sociallyNecessaryLaborTime)
  {
    return new Value(sociallyNecessaryLaborTime);
  }

  /// <summary>
  /// The definite quantitative proportion between two values — what surfaces
  /// as exchange-value when two commodities face each other.
  /// </summary>
  public decimal RatioTo(Value other)
  {
    return Magnitude.RatioTo(other.Magnitude);
  }

  public static Value operator +(Value left, Value right)
  {
    return new Value(left.Magnitude + right.Magnitude);
  }

  /// <summary>
  /// The value of the mass shared out over the things that make it up: what one
  /// of them holds.
  /// </summary>
  public static Value operator /(Value value, decimal divisor)
  {
    return new Value(value.Magnitude / divisor);
  }

  /// <summary>
  /// The value of a mass of one kind: a hundred yards of linen hold a hundred
  /// times the labour of one, for the labour was performed a hundred times over.
  /// </summary>
  public static Value operator *(Value value, decimal factor)
  {
    return new Value(value.Magnitude * factor);
  }

  public override string ToString()
  {
    return $"value of {Magnitude}";
  }

  #endregion

  #region Implementation of IComparable<Value>

  public int CompareTo(Value? other)
  {
    return other is null ? 1 : Magnitude.CompareTo(other.Magnitude);
  }

  #endregion
}
