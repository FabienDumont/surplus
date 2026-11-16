using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class TheFrontMovedTests
{
  #region Tests

  [Theory]
  [InlineData(18, "+18")]
  [InlineData(-7, "-7")]
  [InlineData(0, "0")]
  public void The_front_reads_as_where_the_war_stands(int momentum, string written)
  {
    Assert.Equal(
      $"United Kingdom's war on United States stands at {written}.",
      new TheFrontMoved("United Kingdom", "United States", momentum).ToString());
  }

  #endregion
}
