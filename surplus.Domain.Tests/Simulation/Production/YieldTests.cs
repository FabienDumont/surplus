using Surplus.Domain.SharedKernel;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Production;

public class YieldTests
{
  #region Fields

  private static readonly UnitOfMeasure Yard = UnitOfMeasure.Of("yard");

  #endregion

  #region Tests

  [Fact]
  public void A_period_that_leaves_nothing_behind_has_produced_nothing()
  {
    var linen = CommodityId.New();

    Assert.Throws<DomainException>(() => Yield.Of(Stock.EmptyOf(linen, Yard), Composition()));
  }

  [Fact]
  public void A_yield_reads_as_its_product_what_each_unit_holds_and_the_account_of_it()
  {
    var yield = Yield.Of(Stock.Of(CommodityId.New(), Quantity.Of(240m, Yard)), Composition());

    Assert.Equal("240 yard, each holding 1h of labour (120c + 60v + 60s)", yield.ToString());
  }

  [Fact]
  public void The_individual_value_is_the_whole_of_the_value_spread_over_the_whole_of_the_product()
  {
    var yield = Yield.Of(Stock.Of(CommodityId.New(), Quantity.Of(240m, Yard)), Composition());

    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(240m)), yield.Composition.Product);
    Assert.Equal(Value.CrystallisedFrom(LaborTime.FromHours(1m)), yield.IndividualValue);
  }

  #endregion

  #region Methods

  private static ValueComposition Composition()
  {
    return ValueComposition.FromWorkingDay(
      Value.CrystallisedFrom(LaborTime.FromHours(120m)), LaborTime.FromHours(120m), LaborTime.FromHours(60m)
    );
  }

  #endregion
}
