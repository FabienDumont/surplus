using Surplus.Domain.Simulation.Chronicle;
using Surplus.Domain.Simulation.Commodities;
using Surplus.Domain.Simulation.Production;
using Value = Surplus.Domain.Simulation.Commodities.Value;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class CommodityWasRevaluedTests
{
  #region Tests

  [Fact]
  public void A_fall_in_value_reads_as_how_much_labour_a_unit_now_holds()
  {
    Assert.Equal(
      "Cloth now holds 1.59h of labour per m, down from 2.42h.",
      new CommodityWasRevalued("Cloth", "m", Hours(2.4175m), Hours(1.5873m)).ToString());
  }

  [Fact]
  public void A_rise_in_value_reads_as_such()
  {
    Assert.Equal(
      "Corn now holds 130.45h of labour per t, up from 120.00h.",
      new CommodityWasRevalued("Corn", "t", Hours(120m), Hours(130.4536m)).ToString());
  }

  #endregion

  #region Methods

  private static Value Hours(decimal hours)
  {
    return Value.CrystallisedFrom(LaborTime.FromHours(hours));
  }

  #endregion
}
