using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A magnitude of a use-value, counted in its own unit: twenty yards of linen,
/// one coat, a quarter of wheat.
/// Quantities of one and the same kind add up and stand in definite proportion
/// to one another. Quantities of different kinds do not: as use-values they
/// share no common substance, and only their values are commensurable. The unit
/// is therefore carried here and checked on every operation, so that yards of
/// linen can never be summed with coats.
/// </summary>
public sealed record Quantity : IComparable<Quantity>
{
  #region Properties

  public decimal Amount { get; }

  /// <summary>The natural unit this magnitude is counted in.</summary>
  public UnitOfMeasure Unit { get; }

  public bool IsNone => Amount == 0m;

  #endregion

  #region Ctors

  private Quantity(decimal amount, UnitOfMeasure unit)
  {
    Amount = amount;
    Unit = unit;
  }

  #endregion

  #region Methods

  public static Quantity Of(decimal amount, UnitOfMeasure unit)
  {
    return amount < 0m
      ? throw new DomainException("A quantity of a use-value cannot be negative.")
      : new Quantity(amount, unit);
  }

  /// <summary>None of this use-value — a heap that has been exhausted.</summary>
  public static Quantity NoneOf(UnitOfMeasure unit)
  {
    return new Quantity(0m, unit);
  }

  /// <summary>
  /// The definite proportion between two magnitudes of the same use-value.
  /// </summary>
  public decimal RatioTo(Quantity other)
  {
    RejectAlienUnit(other);

    return other.IsNone
      ? throw new DomainException($"No proportion can be formed with no {Unit} at all.")
      : Amount / other.Amount;
  }

  public static Quantity operator +(Quantity left, Quantity right)
  {
    left.RejectAlienUnit(right);

    return new Quantity(left.Amount + right.Amount, left.Unit);
  }

  public static Quantity operator -(Quantity left, Quantity right)
  {
    left.RejectAlienUnit(right);

    return right.Amount > left.Amount
      ? throw new DomainException($"There is not {right} here to take from {left}.")
      : new Quantity(left.Amount - right.Amount, left.Unit);
  }

  /// <summary>The same use-value in another amount — more linen is still linen.</summary>
  public static Quantity operator *(Quantity quantity, decimal factor)
  {
    return factor < 0m
      ? throw new DomainException("A quantity of a use-value cannot be scaled into the negative.")
      : new Quantity(quantity.Amount * factor, quantity.Unit);
  }

  private void RejectAlienUnit(Quantity other)
  {
    if (Unit != other.Unit)
    {
      throw new DomainException(
        $"Use-values of different kinds are incommensurable: {Unit} and {other.Unit} do not measure one another."
      );
    }
  }

  public override string ToString()
  {
    return $"{Amount.Written()} {Unit}";
  }

  #endregion

  #region Implementation of IComparable<Quantity>

  public int CompareTo(Quantity? other)
  {
    if (other is null)
    {
      return 1;
    }

    RejectAlienUnit(other);

    return Amount.CompareTo(other.Amount);
  }

  #endregion
}
