using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class PopulationGrewTests
{
  #region Tests

  [Fact]
  public void Growth_reads_as_so_many_souls()
  {
    Assert.Equal("United Kingdom grew by 1,198 souls.", new PopulationGrew("United Kingdom", 1_198).ToString());
  }

  [Fact]
  public void One_soul_is_a_soul()
  {
    Assert.Equal("Russia grew by 1 soul.", new PopulationGrew("Russia", 1).ToString());
  }

  #endregion
}
