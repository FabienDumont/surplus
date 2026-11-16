using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class SankIntoPauperismTests
{
  #region Tests

  [Fact]
  public void Pauperism_reads_as_how_many_sank()
  {
    Assert.Equal("In United Kingdom 1,034 sank into pauperism.", new SankIntoPauperism("United Kingdom", 1_034).ToString());
  }

  #endregion
}
