using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;

namespace Surplus.Domain.Simulation.Production;

/// <summary>
/// How the value of a product breaks up — the formula the whole simulation turns
/// on, written as Marx writes it: c + v + s.
/// c, constant capital, is the value the means of production hand on. It is
/// constant precisely because it changes no magnitude in the process: it is
/// preserved, transferred, made to reappear, never increased.
/// v, variable capital, is what the buyer lays out on labour-power. It alone is
/// variable, because the one commodity whose use-value is the source of value is
/// the capacity to labour, and it gives up more than it costs.
/// s, surplus-value, is that excess: unpaid labour, and the sole reason the
/// process is entered into at all.
/// v and s together are the whole of the new value, and living labour is the
/// whole of what produces them. Nothing here can create value out of c.
/// </summary>
public sealed record ValueComposition
{
  #region Properties

  /// <summary>c — dead labour, preserved and handed on.</summary>
  public Value ConstantCapital { get; }

  /// <summary>v — what labour-power cost.</summary>
  public Value VariableCapital { get; }

  /// <summary>s — the labour performed over and above that, and not paid for.</summary>
  public Value SurplusValue { get; }

  /// <summary>v + s: everything the living factor added, paid part and unpaid part alike.</summary>
  public Value NewValue => VariableCapital + SurplusValue;

  /// <summary>c + v + s: the value of the product.</summary>
  public Value Product => ConstantCapital + NewValue;

  /// <summary>Whether anyone here works for someone else at all.</summary>
  public bool ExtractsSurplus => !SurplusValue.IsNone;

  #endregion

  #region Ctors

  private ValueComposition(Value constantCapital, Value variableCapital, Value surplusValue)
  {
    ConstantCapital = constantCapital;
    VariableCapital = variableCapital;
    SurplusValue = surplusValue;
  }

  #endregion

  #region Methods

  public static ValueComposition Of(Value constantCapital, Value variableCapital, Value surplusValue)
  {
    return new ValueComposition(constantCapital, variableCapital, surplusValue);
  }

  /// <summary>
  /// The composition as it comes out of a working day actually worked. The means
  /// of production hand on c; the living labour adds the whole of v + s, and the
  /// line between them falls where necessary labour ends — the hours in which
  /// the labourer produces no more than the value of their own keep. The rest is
  /// surplus labour.
  /// Nothing in the working day marks that point. The labourer does not work the
  /// first half for themselves and the second for the master and know it: the
  /// wage is paid for the day entire, and makes every hour of it look paid.
  /// That appearance is the whole secret of the form, and the reason the split
  /// has to be computed here rather than observed.
  /// </summary>
  public static ValueComposition FromWorkingDay(
    Value constantCapital, LaborTime workingDay, LaborTime necessaryLabor)
  {
    if (workingDay.IsNone)
    {
      throw new DomainException("No new value arises where no labour is performed.");
    }

    if (necessaryLabor.CompareTo(workingDay) > 0)
    {
      throw new DomainException(
        "Necessary labour is a part of the working day: a day too short to reproduce the labourer " +
        "consumes them instead of employing them."
      );
    }

    return new ValueComposition(
      constantCapital, Value.CrystallisedFrom(necessaryLabor),
      Value.CrystallisedFrom(workingDay - necessaryLabor)
    );
  }

  /// <summary>
  /// s / v — the rate of surplus-value, and the exact expression of the degree
  /// of exploitation. Not s / (c + v): capital laid out on means of production
  /// is exploited by nobody, and reckoning the surplus against it only conceals
  /// whose labour produced it.
  /// </summary>
  public decimal RateOfSurplusValue()
  {
    return VariableCapital.IsNone
      ? throw new DomainException(
        "Where no labour-power is bought there is no rate of surplus-value: the slave is bought outright, " +
        "and the whole of their labour appears unpaid."
      )
      : SurplusValue.RatioTo(VariableCapital);
  }

  /// <summary>
  /// c / v — the composition of capital, in so far as it mirrors the technical
  /// one: how much dead labour each unit of living labour sets in motion. It
  /// rises as machinery spreads, and everything else follows from that rise.
  /// </summary>
  public decimal OrganicComposition()
  {
    return VariableCapital.IsNone
      ? throw new DomainException("Nothing is set in motion here: there is no living labour to compose against.")
      : ConstantCapital.RatioTo(VariableCapital);
  }

  /// <summary>
  /// s / (c + v) — the rate of profit, which is how the capitalist sees the same
  /// surplus, and sees it wrong: it credits the whole capital with what only the
  /// living part produced. Its tendency to fall as c / v rises is not a flaw in
  /// the arithmetic but the arithmetic of the thing itself.
  /// </summary>
  public decimal RateOfProfit()
  {
    var advanced = ConstantCapital + VariableCapital;

    return advanced.IsNone
      ? throw new DomainException("No capital was advanced here, and none can return a profit.")
      : SurplusValue.RatioTo(advanced);
  }

  public override string ToString()
  {
    return $"{ConstantCapital.Magnitude.Hours.Written()}c + {VariableCapital.Magnitude.Hours.Written()}v + " +
           $"{SurplusValue.Magnitude.Hours.Written()}s";
  }

  #endregion
}
