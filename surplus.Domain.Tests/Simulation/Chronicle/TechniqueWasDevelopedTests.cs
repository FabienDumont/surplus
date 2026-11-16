using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class TechniqueWasDevelopedTests
{
  #region Tests

  [Fact]
  public void A_technique_found_reads_as_a_new_way_of_making_something()
  {
    Assert.Equal(
      "Russia has found out how to work by Large farming, a new way of making Corn.",
      new TechniqueWasDeveloped("Russia", "Large farming", "Corn").ToString());
  }

  #endregion
}
