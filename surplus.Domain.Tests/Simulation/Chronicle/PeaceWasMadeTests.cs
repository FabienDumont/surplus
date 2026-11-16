using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class PeaceWasMadeTests
{
  #region Tests

  [Fact]
  public void A_peace_reads_as_the_parties_and_the_terms()
  {
    Assert.Equal(
      "United Kingdom and United States have made peace: Carolina is ceded to United Kingdom.",
      new PeaceWasMade("United Kingdom", "United States", "Carolina is ceded to United Kingdom").ToString());
  }

  #endregion
}
