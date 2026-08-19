using Surplus.Domain.Simulation.Production;

namespace Surplus.Testing;

/// <summary>Builds a <see cref="UseValue" />, by default the utility of a coat.</summary>
public sealed class UseValueBuilder
{
  #region Fields

  private string _satisfiedWant = "clothing";
  private UnitOfMeasure _unit = UnitOfMeasure.Of("coat");

  #endregion

  #region Methods

  public UseValueBuilder WithSatisfiedWant(string satisfiedWant)
  {
    _satisfiedWant = satisfiedWant;

    return this;
  }

  public UseValueBuilder WithUnit(UnitOfMeasure unit)
  {
    _unit = unit;

    return this;
  }

  public UseValueBuilder WithUnit(string unitName)
  {
    return WithUnit(UnitOfMeasure.Of(unitName));
  }

  public UseValue Build()
  {
    return UseValue.Of(_satisfiedWant, _unit);
  }

  #endregion
}
