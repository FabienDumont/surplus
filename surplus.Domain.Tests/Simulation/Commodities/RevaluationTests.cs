using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Commodities;

public class RevaluationTests
{
  #region Tests

  [Fact]
  public void A_value_that_rose_changed_by_a_positive_share_of_what_it_was()
  {
    Assert.Equal(0.25m, new Revaluation(CommodityId.New(), Hours(4m), Hours(5m)).RelativeChange);
  }

  [Fact]
  public void A_value_that_held_did_not_change()
  {
    Assert.Equal(0m, new Revaluation(CommodityId.New(), Hours(4m), Hours(4m)).RelativeChange);
  }

  #endregion

  #region Methods

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  #endregion
}
