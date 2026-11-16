using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class TheWorklessWentWithoutTests
{
  #region Tests

  [Fact]
  public void Worklessness_reads_as_how_many_had_no_wage_and_how_many_died()
  {
    Assert.Equal(
      "In United Kingdom 3,350 had no wage to live on; 16 died.",
      new TheWorklessWentWithout("United Kingdom", 3_350, 16).ToString());
  }

  [Fact]
  public void A_day_without_deaths_says_so()
  {
    Assert.Equal(
      "In United States 100 had no wage to live on; no one died of it today.",
      new TheWorklessWentWithout("United States", 100, 0).ToString());
  }

  #endregion
}
