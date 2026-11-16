using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Production;

public class DayOfProductionTests
{
  #region Fields

  private static readonly CommodityId Cloth = CommodityId.New();
  private static readonly ProvinceId Lancashire = ProvinceId.New();
  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void Two_days_that_came_to_the_same_things_are_the_same_day_of_production()
  {
    var day = new DayOfProduction([Woven(48m)], [new IdleBranch(Lancashire, Cloth)]);
    var same = new DayOfProduction([Woven(48m)], [new IdleBranch(Lancashire, Cloth)]);

    Assert.Equal(day, same);
    Assert.Equal(day.GetHashCode(), same.GetHashCode());
    Assert.NotEqual(day, new DayOfProduction([Woven(24m)], [new IdleBranch(Lancashire, Cloth)]));
    Assert.NotEqual(day, new DayOfProduction([Woven(48m)], []));
    Assert.False(day.Equals(null));
  }

  #endregion

  #region Methods

  private static Yield Woven(decimal yards)
  {
    return Yield.Of(
      Stock.Of(Cloth, Quantity.Of(yards, Yard)),
      ValueComposition.FromWorkingDay(Value.None, LaborTime.FromHours(24m), LaborTime.FromHours(12m)));
  }

  #endregion
}
