using Surplus.Domain.Simulation.Chronicle;

namespace Surplus.Domain.Tests.Simulation.Chronicle;

public class BranchStoodIdleTests
{
  #region Tests

  [Fact]
  public void An_idle_branch_reads_as_the_sentence_a_player_is_told()
  {
    Assert.Equal(
      "Lancashire's Cloth branch stood idle for want of material (United Kingdom).",
      new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth").ToString());
  }

  [Fact]
  public void The_same_branch_standing_idle_elsewhere_is_another_occurrence()
  {
    var lancashire = new BranchStoodIdle("United Kingdom", "Lancashire", "Cloth");

    var normandie = lancashire with { Country = "France", Province = "Normandie" };

    Assert.NotEqual(lancashire, normandie);
    Assert.Equal("Normandie's Cloth branch stood idle for want of material (France).", normandie.ToString());
  }

  #endregion
}
