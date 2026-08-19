using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;

namespace Surplus.Testing;

/// <summary>
/// Builds a <see cref="Commodity" /> directly in whatever state a test needs,
/// bypassing the checks <see cref="Commodity.Produce" /> makes at birth.
/// </summary>
public sealed class CommodityBuilder
{
  #region Fields

  private Department _department = Department.MeansOfConsumption;
  private CommodityId _id = CommodityId.New();
  private string _name = "Coat";
  private UseValue _useValue = new UseValueBuilder().Build();
  private Value _value = Value.CrystallisedFrom(LaborTime.FromHours(20m));

  #endregion

  #region Methods

  public CommodityBuilder WithId(CommodityId id)
  {
    _id = id;

    return this;
  }

  public CommodityBuilder WithName(string name)
  {
    _name = name;

    return this;
  }

  public CommodityBuilder WithDepartment(Department department)
  {
    _department = department;

    return this;
  }

  public CommodityBuilder WithUseValue(UseValue useValue)
  {
    _useValue = useValue;

    return this;
  }

  public CommodityBuilder WithValue(Value value)
  {
    _value = value;

    return this;
  }

  /// <summary>
  /// Sets the value by the route the domain recognises: the labour time
  /// socially necessary to produce the commodity.
  /// </summary>
  public CommodityBuilder WithSociallyNecessaryLaborTime(LaborTime sociallyNecessaryLaborTime)
  {
    return WithValue(Value.CrystallisedFrom(sociallyNecessaryLaborTime));
  }

  public Commodity Build()
  {
    return Commodity.Load(_id, _name, _useValue, _department, _value);
  }

  #endregion
}
