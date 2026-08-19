using Surplus.Domain.SharedKernel;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// A quantity of undifferentiated, homogeneous human labour.
/// Socially necessary labour time is the measure of the magnitude of value.
/// </summary>
public readonly record struct LaborTime : IComparable<LaborTime>
{
  #region Fields

  public static readonly LaborTime None = new(0m);

  #endregion

  #region Properties

  public decimal Hours { get; }

  public bool IsNone => Hours == 0m;

  #endregion

  #region Ctors

  private LaborTime(decimal hours)
  {
    Hours = hours;
  }

  #endregion

  #region Methods

  public static LaborTime FromHours(decimal hours)
  {
    return hours < 0m ? throw new DomainException("Labour time cannot be negative.") : new LaborTime(hours);
  }

  public static LaborTime operator +(LaborTime left, LaborTime right)
  {
    return new LaborTime(left.Hours + right.Hours);
  }

  public static LaborTime operator -(LaborTime left, LaborTime right)
  {
    return right.Hours > left.Hours
      ? throw new DomainException("No more labour can be taken out of a span than was performed in it.")
      : new LaborTime(left.Hours - right.Hours);
  }

  /// <summary>
  /// The labour spread over a number of things — how much of it fell to each.
  /// </summary>
  public static LaborTime operator /(LaborTime laborTime, decimal divisor)
  {
    return divisor <= 0m
      ? throw new DomainException("Labour cannot be spread over nothing.")
      : new LaborTime(laborTime.Hours / divisor);
  }

  /// <summary>
  /// So many times the same labour. Repeating a working day does not change the
  /// kind of labour performed, only how much of it has been spent.
  /// </summary>
  public static LaborTime operator *(LaborTime laborTime, decimal factor)
  {
    return factor < 0m
      ? throw new DomainException("Labour time cannot be scaled into the negative.")
      : new LaborTime(laborTime.Hours * factor);
  }

  /// <summary>
  /// Labour times are homogeneous, so any two of them stand in a definite
  /// quantitative proportion to one another.
  /// </summary>
  public decimal RatioTo(LaborTime other)
  {
    return other.IsNone
      ? throw new DomainException("No proportion can be formed with a thing that contains no labour.")
      : Hours / other.Hours;
  }

  public override string ToString()
  {
    return $"{Hours.Written()}h of labour";
  }

  #endregion

  #region Implementation of IComparable<LaborTime>

  public int CompareTo(LaborTime other)
  {
    return Hours.CompareTo(other.Hours);
  }

  #endregion
}
