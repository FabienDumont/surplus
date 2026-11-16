using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class WarWasDeclaredTests
{
  #region Tests

  [Fact]
  public void A_declaration_names_who_attacked_whom()
  {
    Assert.Equal("Russia has declared war on United States.", new WarWasDeclared("Russia", "United States").ToString());
  }

  #endregion
}
